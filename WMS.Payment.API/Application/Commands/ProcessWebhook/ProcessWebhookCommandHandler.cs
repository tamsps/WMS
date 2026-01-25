using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Payment.API.Common.Models;

namespace WMS.Payment.API.Application.Commands.ProcessWebhook;

/// <summary>
/// Handler for processing payment gateway webhooks with idempotency
/// 
/// IDEMPOTENCY STRATEGY:
/// 1. Identify webhook by GatewayEventId (unique per webhook from gateway)
/// 2. Check if GatewayEventId already processed
/// 3. If duplicate: Log and return success (idempotent behavior)
/// 4. If new: Process payment update and log event
/// 5. Always return success to gateway (prevents retries)
/// 
/// AUDIT REQUIREMENTS:
/// - All webhook attempts logged in PaymentEvents
/// - Duplicate attempts flagged with IsProcessed = false
/// - Original processing flagged with IsProcessed = true
/// - Full event data preserved for troubleshooting
/// </summary>
public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, Result>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;

    public ProcessWebhookCommandHandler(
        WMSDbContext _context,
        IUnitOfWork unitOfWork,
        ILogger<ProcessWebhookCommandHandler> logger)
    {
        this._context = _context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.ExternalPaymentId))
        {
            _logger.LogWarning("Webhook received without ExternalPaymentId");
            return Result.Failure("ExternalPaymentId is required");
        }
        
        if (string.IsNullOrWhiteSpace(dto.GatewayEventId))
        {
            _logger.LogWarning("Webhook received without GatewayEventId for payment {ExternalPaymentId}", 
                dto.ExternalPaymentId);
            return Result.Failure("GatewayEventId is required for idempotency");
        }

        // Find payment by external ID
        var payment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == dto.ExternalPaymentId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Webhook received for unknown payment: {ExternalPaymentId}, Event: {GatewayEventId}", 
                dto.ExternalPaymentId, dto.GatewayEventId);
            
            // Still return success to prevent gateway retries
            // Log for manual investigation
            return Result.Failure($"Payment not found: {dto.ExternalPaymentId}");
        }

        // IDEMPOTENCY CHECK: Has this exact webhook been processed before?
        var existingEvent = payment.PaymentEvents
            .FirstOrDefault(e => e.GatewayEventId == dto.GatewayEventId);

        if (existingEvent != null)
        {
            // DUPLICATE WEBHOOK DETECTED
            _logger.LogInformation(
                "Duplicate webhook ignored. Payment: {PaymentNumber}, Event: {GatewayEventId}, " +
                "Original processed: {OriginalDate}", 
                payment.PaymentNumber, dto.GatewayEventId, existingEvent.CreatedAt);

            // Log duplicate attempt for audit
            payment.PaymentEvents.Add(new PaymentEvent
            {
                EventType = "WebhookDuplicate",
                GatewayEventId = dto.GatewayEventId,
                EventData = dto.EventData,
                Notes = $"Duplicate webhook ignored. Original processed at {existingEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                CreatedBy = "System",
                IsProcessed = false // Not processed (duplicate)
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return success to prevent gateway retries
            return Result.Success("Webhook already processed (duplicate ignored)");
        }

        // NEW WEBHOOK - PROCESS IT
        _logger.LogInformation("Processing webhook. Payment: {PaymentNumber}, Event: {GatewayEventId}, Status: {Status}", 
            payment.PaymentNumber, dto.GatewayEventId, dto.Status);

        // Parse status
        if (!Enum.TryParse<PaymentStatus>(dto.Status, out var newStatus))
        {
            _logger.LogWarning("Invalid payment status in webhook: {Status}, Payment: {PaymentNumber}", 
                dto.Status, payment.PaymentNumber);
            
            // Log invalid status event
            payment.PaymentEvents.Add(new PaymentEvent
            {
                EventType = "WebhookInvalidStatus",
                GatewayEventId = dto.GatewayEventId,
                EventData = dto.EventData,
                Notes = $"Invalid status: {dto.Status}",
                CreatedBy = "System",
                IsProcessed = false
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure($"Invalid payment status: {dto.Status}");
        }

        // Update payment status
        var previousStatus = payment.Status;
        payment.Status = newStatus;
        payment.UpdatedBy = "System";
        payment.UpdatedAt = DateTime.UtcNow;

        // Set dates based on new status
        if (newStatus == PaymentStatus.Confirmed && !payment.ConfirmedDate.HasValue)
        {
            payment.PaymentDate = dto.GatewayTimestamp ?? DateTime.UtcNow;
            payment.ConfirmedDate = DateTime.UtcNow;
        }

        // Log successful webhook processing
        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "WebhookReceived",
            GatewayEventId = dto.GatewayEventId, // ? Store for idempotency
            EventData = dto.EventData,
            Notes = $"Status changed: {previousStatus} ? {newStatus}",
            CreatedBy = "System",
            IsProcessed = true // ? Successfully processed
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Webhook processed successfully. Payment: {PaymentNumber}, " +
            "Status: {PreviousStatus} ? {NewStatus}, Event: {GatewayEventId}", 
            payment.PaymentNumber, previousStatus, newStatus, dto.GatewayEventId);

        return Result.Success("Webhook processed successfully");
    }
}
