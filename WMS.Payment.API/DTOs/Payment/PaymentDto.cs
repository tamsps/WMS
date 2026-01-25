namespace WMS.Payment.API.DTOs.Payment;

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
    
    /// <summary>
    /// Payment URL for customer redirect (from gateway initiation)
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// Payment session ID from gateway
    /// </summary>
    public string? SessionId { get; set; }
    
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

/// <summary>
/// Initiate payment DTO - Request payment session from gateway
/// </summary>
public class InitiatePaymentDto
{
    /// <summary>
    /// Payment ID in WMS
    /// </summary>
    public Guid PaymentId { get; set; }
    
    /// <summary>
    /// Payment gateway to use (Stripe, PayPal, Square, etc.)
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;
    
    /// <summary>
    /// Success URL - where to redirect customer after successful payment
    /// </summary>
    public string? SuccessUrl { get; set; }
    
    /// <summary>
    /// Cancel URL - where to redirect customer if payment is cancelled
    /// </summary>
    public string? CancelUrl { get; set; }
    
    /// <summary>
    /// Customer email for receipt
    /// </summary>
    public string? CustomerEmail { get; set; }
    
    /// <summary>
    /// Additional metadata for gateway
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Payment initiation response - Contains payment URL for customer
/// </summary>
public class InitiatePaymentResponseDto
{
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Payment ID in WMS
    /// </summary>
    public Guid PaymentId { get; set; }
    
    /// <summary>
    /// Payment URL where customer should be redirected
    /// Customer completes payment on gateway page
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// Session ID from gateway
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// External payment ID from gateway
    /// </summary>
    public string? ExternalPaymentId { get; set; }
    
    /// <summary>
    /// QR code for mobile payments (optional)
    /// </summary>
    public string? QrCode { get; set; }
    
    /// <summary>
    /// Session expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error message if initiation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

public class ConfirmPaymentDto
{
    public Guid PaymentId { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? TransactionReference { get; set; }
}

/// <summary>
/// Payment webhook DTO for processing gateway callbacks
/// 
/// IDEMPOTENCY:
/// - GatewayEventId must be unique for each webhook
/// - Same GatewayEventId received multiple times = duplicate (safe to ignore)
/// - ExternalPaymentId identifies the payment
/// </summary>
public class PaymentWebhookDto
{
    /// <summary>
    /// External payment ID from gateway (identifies the payment)
    /// Required to find the payment record
    /// </summary>
    public string ExternalPaymentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique event ID from payment gateway
    /// Used for idempotency - prevents duplicate processing
    /// Examples: stripe_event_123, paypal_ipn_456, gateway_webhook_789
    /// </summary>
    public string GatewayEventId { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment status from webhook: Confirmed, Failed, Cancelled, etc.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Full JSON payload from gateway (for audit/debugging)
    /// </summary>
    public string EventData { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp from gateway (optional)
    /// </summary>
    public DateTime? GatewayTimestamp { get; set; }
}
