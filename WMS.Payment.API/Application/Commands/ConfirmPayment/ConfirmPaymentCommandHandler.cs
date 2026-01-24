using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Payment.API.Application.Mappers;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result<PaymentDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmPaymentCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentDto>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Outbound)
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.Id == request.Dto.PaymentId, cancellationToken);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result<PaymentDto>.Failure($"Cannot confirm payment in {payment.Status} status");
        }

        payment.Status = PaymentStatus.Confirmed;
        payment.ConfirmedDate = DateTime.UtcNow;
        payment.ExternalPaymentId = request.Dto.ExternalPaymentId;
        payment.TransactionReference = request.Dto.TransactionReference;
        payment.UpdatedBy = request.CurrentUser;
        payment.UpdatedAt = DateTime.UtcNow;

        // Add confirmation event
        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Confirmed",
            EventData = $"Payment confirmed. Transaction: {request.Dto.TransactionReference}",
            Notes = "Payment confirmed successfully",
            CreatedBy = request.CurrentUser
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(
            PaymentMapper.MapToDto(payment),
            "Payment confirmed successfully");
    }
}
