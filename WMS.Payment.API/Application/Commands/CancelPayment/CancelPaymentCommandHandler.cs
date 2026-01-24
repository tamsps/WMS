using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Payment.API.Application.Mappers;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.CancelPayment;

public class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand, Result<PaymentDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPaymentCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentDto>> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Outbound)
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        if (payment.Status == PaymentStatus.Confirmed)
        {
            return Result<PaymentDto>.Failure("Cannot cancel a confirmed payment");
        }

        payment.Status = PaymentStatus.Cancelled;
        payment.UpdatedBy = request.CurrentUser;
        payment.UpdatedAt = DateTime.UtcNow;

        // Add cancellation event
        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Cancelled",
            EventData = "Payment cancelled",
            Notes = "Payment cancelled by user",
            CreatedBy = request.CurrentUser
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentDto>.Success(
            PaymentMapper.MapToDto(payment),
            "Payment cancelled successfully");
    }
}
