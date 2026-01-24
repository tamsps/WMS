using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Mappers;

public static class PaymentMapper
{
    public static PaymentDto MapToDto(WMS.Domain.Entities.Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            OutboundId = payment.OutboundId,
            OutboundNumber = payment.Outbound?.OutboundNumber,
            PaymentType = payment.PaymentType.ToString(),
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            ExternalPaymentId = payment.ExternalPaymentId,
            PaymentGateway = payment.PaymentGateway,
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = payment.PaymentDate,
            ConfirmedDate = payment.ConfirmedDate,
            TransactionReference = payment.TransactionReference,
            CreatedAt = payment.CreatedAt
        };
    }
}
