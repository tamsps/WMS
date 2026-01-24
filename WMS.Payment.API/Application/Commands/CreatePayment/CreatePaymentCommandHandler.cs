using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Payment.API.Application.Mappers;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<WMS.Domain.Entities.Payment> _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePaymentCommandHandler(
        WMSDbContext context,
        IRepository<WMS.Domain.Entities.Payment> paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Validate outbound if specified
        if (request.Dto.OutboundId.HasValue)
        {
            var outbound = await _context.Outbounds
                .FirstOrDefaultAsync(o => o.Id == request.Dto.OutboundId.Value, cancellationToken);

            if (outbound == null)
            {
                return Result<PaymentDto>.Failure("Outbound not found");
            }

            // Check if payment already exists for this outbound
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OutboundId == request.Dto.OutboundId.Value, cancellationToken);

            if (existingPayment != null)
            {
                return Result<PaymentDto>.Failure("Payment already exists for this outbound");
            }
        }

        // Parse payment type
        if (!Enum.TryParse<PaymentType>(request.Dto.PaymentType, out var paymentType))
        {
            return Result<PaymentDto>.Failure($"Invalid payment type: {request.Dto.PaymentType}");
        }

        var payment = new WMS.Domain.Entities.Payment
        {
            PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken),
            OutboundId = request.Dto.OutboundId,
            PaymentType = paymentType,
            Status = PaymentStatus.Pending,
            Amount = request.Dto.Amount,
            Currency = request.Dto.Currency,
            PaymentMethod = request.Dto.PaymentMethod,
            CreatedBy = request.CurrentUser
        };

        // Add initial event
        payment.PaymentEvents.Add(new PaymentEvent
        {
            EventType = "Created",
            EventData = $"Payment created with amount {payment.Amount} {payment.Currency}",
            Notes = "Payment record created",
            CreatedBy = request.CurrentUser
        });

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with includes
        var created = await _context.Payments
            .Include(p => p.Outbound)
            .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

        return Result<PaymentDto>.Success(
            PaymentMapper.MapToDto(created!),
            "Payment created successfully");
    }

    private async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PAY-{today:yyyyMMdd}";

        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastPayment == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastPayment.PaymentNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
