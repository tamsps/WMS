namespace WMS.Application.DTOs.Payment;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? OutboundId { get; set; }
    public string? OutboundNumber { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ExternalPaymentId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentDto
{
    public Guid? OutboundId { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PaymentMethod { get; set; }
}

public class InitiatePaymentDto
{
    public Guid PaymentId { get; set; }
    public string PaymentGateway { get; set; } = string.Empty;
}

public class ConfirmPaymentDto
{
    public Guid PaymentId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? TransactionReference { get; set; }
}

public class PaymentWebhookDto
{
    public string? ExternalPaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
}
