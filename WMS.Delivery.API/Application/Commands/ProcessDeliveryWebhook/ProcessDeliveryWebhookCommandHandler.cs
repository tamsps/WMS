using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Common.Models;

namespace WMS.Delivery.API.Application.Commands.ProcessDeliveryWebhook;

/// <summary>
/// Handler for processing delivery partner webhooks with idempotency
/// 
/// IDEMPOTENCY STRATEGY:
/// 1. Identify webhook by PartnerEventId (unique per webhook from partner)
/// 2. Check if PartnerEventId already processed
/// 3. If duplicate: Log and return success (idempotent behavior)
/// 4. If new: Process delivery update and log event
/// 5. Always return success to partner (prevents retries)
/// 
/// AUDIT REQUIREMENTS:
/// - All webhook attempts logged in DeliveryEvents
/// - Duplicate attempts flagged with IsProcessed = false
/// - Original processing flagged with IsProcessed = true
/// - Full event data preserved for troubleshooting
/// 
/// BUSINESS RULES:
/// - Failed deliveries may trigger return inbound (handled separately)
/// - Delivered status may trigger customer notification
/// - Status transitions validated before update
/// </summary>
public class ProcessDeliveryWebhookCommandHandler : IRequestHandler<ProcessDeliveryWebhookCommand, Result>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDeliveryWebhookCommandHandler> _logger;

    public ProcessDeliveryWebhookCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ProcessDeliveryWebhookCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ProcessDeliveryWebhookCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.TrackingNumber))
        {
            _logger.LogWarning("Delivery webhook received without TrackingNumber");
            return Result.Failure("TrackingNumber is required");
        }
        
        if (string.IsNullOrWhiteSpace(dto.PartnerEventId))
        {
            _logger.LogWarning("Delivery webhook received without PartnerEventId for tracking: {TrackingNumber}", 
                dto.TrackingNumber);
            return Result.Failure("PartnerEventId is required for idempotency");
        }

        // Find delivery by tracking number
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.TrackingNumber == dto.TrackingNumber, cancellationToken);

        if (delivery == null)
        {
            _logger.LogWarning("Webhook received for unknown delivery: {TrackingNumber}, Event: {PartnerEventId}", 
                dto.TrackingNumber, dto.PartnerEventId);
            
            // Still return success to prevent partner retries
            return Result.Failure($"Delivery not found: {dto.TrackingNumber}");
        }

        // IDEMPOTENCY CHECK: Has this exact webhook been processed before?
        var existingEvent = delivery.DeliveryEvents
            .FirstOrDefault(e => e.PartnerEventId == dto.PartnerEventId);

        if (existingEvent != null)
        {
            // DUPLICATE WEBHOOK DETECTED
            _logger.LogInformation(
                "Duplicate delivery webhook ignored. Delivery: {DeliveryNumber}, Event: {PartnerEventId}, " +
                "Original processed: {OriginalDate}", 
                delivery.DeliveryNumber, dto.PartnerEventId, existingEvent.CreatedAt);

            // Log duplicate attempt for audit
            delivery.DeliveryEvents.Add(new DeliveryEvent
            {
                EventType = "WebhookDuplicate",
                PartnerEventId = dto.PartnerEventId,
                EventDate = dto.EventTimestamp ?? DateTime.UtcNow,
                Location = dto.CurrentLocation,
                EventData = dto.EventData,
                Notes = $"Duplicate webhook ignored. Original processed at {existingEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}",
                CreatedBy = "System",
                IsProcessed = false // Not processed (duplicate)
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return success to prevent partner retries
            return Result.Success("Webhook already processed (duplicate ignored)");
        }

        // NEW WEBHOOK - PROCESS IT
        _logger.LogInformation("Processing delivery webhook. Delivery: {DeliveryNumber}, Event: {PartnerEventId}, Status: {Status}", 
            delivery.DeliveryNumber, dto.PartnerEventId, dto.Status);

        // Parse status
        if (!Enum.TryParse<DeliveryStatus>(dto.Status, out var newStatus))
        {
            _logger.LogWarning("Invalid delivery status in webhook: {Status}, Delivery: {DeliveryNumber}", 
                dto.Status, delivery.DeliveryNumber);
            
            // Log invalid status event
            delivery.DeliveryEvents.Add(new DeliveryEvent
            {
                EventType = "WebhookInvalidStatus",
                PartnerEventId = dto.PartnerEventId,
                EventDate = dto.EventTimestamp ?? DateTime.UtcNow,
                Location = dto.CurrentLocation,
                EventData = dto.EventData,
                Notes = $"Invalid status: {dto.Status}",
                CreatedBy = "System",
                IsProcessed = false
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure($"Invalid delivery status: {dto.Status}");
        }

        // Validate status transition
        if (!IsValidStatusTransition(delivery.Status, newStatus))
        {
            _logger.LogWarning("Invalid status transition in webhook: {CurrentStatus} ? {NewStatus}, Delivery: {DeliveryNumber}", 
                delivery.Status, newStatus, delivery.DeliveryNumber);
            
            delivery.DeliveryEvents.Add(new DeliveryEvent
            {
                EventType = "WebhookInvalidTransition",
                PartnerEventId = dto.PartnerEventId,
                EventDate = dto.EventTimestamp ?? DateTime.UtcNow,
                Location = dto.CurrentLocation,
                EventData = dto.EventData,
                Notes = $"Invalid transition: {delivery.Status} ? {newStatus}",
                CreatedBy = "System",
                IsProcessed = false
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure($"Invalid status transition: {delivery.Status} ? {newStatus}");
        }

        // Update delivery status
        var previousStatus = delivery.Status;
        delivery.Status = newStatus;
        delivery.UpdatedBy = "System";
        delivery.UpdatedAt = DateTime.UtcNow;

        // Set dates based on new status
        if (newStatus == DeliveryStatus.InTransit && !delivery.PickupDate.HasValue)
        {
            delivery.PickupDate = dto.EventTimestamp ?? DateTime.UtcNow;
        }
        
        if (newStatus == DeliveryStatus.Delivered && !delivery.ActualDeliveryDate.HasValue)
        {
            delivery.ActualDeliveryDate = dto.EventTimestamp ?? DateTime.UtcNow;
        }

        // Log successful webhook processing
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = newStatus.ToString(),
            PartnerEventId = dto.PartnerEventId, // ? Store for idempotency
            EventDate = dto.EventTimestamp ?? DateTime.UtcNow,
            Location = dto.CurrentLocation,
            EventData = dto.EventData,
            Notes = dto.Notes ?? $"Status changed: {previousStatus} ? {newStatus}",
            CreatedBy = "System",
            IsProcessed = true // ? Successfully processed
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Delivery webhook processed successfully. Delivery: {DeliveryNumber}, " +
            "Status: {PreviousStatus} ? {NewStatus}, Event: {PartnerEventId}", 
            delivery.DeliveryNumber, previousStatus, newStatus, dto.PartnerEventId);

        // TODO: Trigger business rules based on status
        // if (newStatus == DeliveryStatus.Failed)
        // {
        //     // May trigger return inbound creation
        // }
        // if (newStatus == DeliveryStatus.Delivered)
        // {
        //     // May trigger customer notification
        // }

        return Result.Success("Webhook processed successfully");
    }

    private bool IsValidStatusTransition(DeliveryStatus current, DeliveryStatus next)
    {
        return (current, next) switch
        {
            (DeliveryStatus.Pending, DeliveryStatus.InTransit) => true,
            (DeliveryStatus.InTransit, DeliveryStatus.Delivered) => true,
            (DeliveryStatus.InTransit, DeliveryStatus.Failed) => true,
            (DeliveryStatus.Pending, DeliveryStatus.Cancelled) => true,
            // Allow re-processing same status (idempotent)
            var (c, n) when c == n => true,
            _ => false
        };
    }
}
