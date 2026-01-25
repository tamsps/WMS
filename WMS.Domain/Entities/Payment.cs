using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Payment Entity - Operational payment state management
/// Responsible for payment state and shipment gating, not financial settlement
/// 
/// GATEWAY INTEGRATION:
/// - Initiation: WMS creates payment session with gateway
/// - Customer Payment: User pays on gateway page/app
/// - Webhook Callback: Gateway notifies WMS asynchronously
/// - Verification: WMS verifies status with gateway API
/// - Idempotency: Duplicate webhooks are safely ignored
/// 
/// ASYNCHRONOUS DESIGN:
/// - Payment is NOT part of inventory transactions
/// - Decoupled from warehouse operations
/// - No blocking on payment completion
/// </summary>
public class Payment : BaseEntity
{
    public string PaymentNumber { get; set; } = string.Empty;

    // Optional link to an outbound shipment/order
    public Guid? OutboundId { get; set; }
    public virtual Outbound? Outbound { get; set; }
    
    public PaymentType PaymentType { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// External payment gateway transaction ID
    /// Used to identify payment across systems
    /// Set after initiation or webhook
    /// </summary>
    public string? ExternalPaymentId { get; set; }
    
    /// <summary>
    /// Payment session ID from gateway
    /// Used to track payment session
    /// Set during initiation
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Payment gateway name (Stripe, PayPal, Square, etc.)
    /// </summary>
    public string? PaymentGateway { get; set; }
    
    /// <summary>
    /// Payment method used (Credit Card, Bank Transfer, etc.)
    /// Set after payment completion
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Payment URL where customer completes payment
    /// Generated during initiation
    /// Customer is redirected to this URL
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// Payment session expiration time
    /// Set during initiation
    /// </summary>
    public DateTime? SessionExpiresAt { get; set; }
    
    public DateTime? PaymentDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    
    // Audit trail for payment events
    public virtual ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
}

/// <summary>
/// Payment Event Entity - Tracks all payment state changes and webhook deliveries
/// 
/// IDEMPOTENCY MECHANISM:
/// - Each webhook has unique GatewayEventId
/// - EventType + GatewayEventId must be unique per payment
/// - Duplicate webhooks with same GatewayEventId are ignored
/// - All webhook attempts are logged (success and duplicate)
/// 
/// EVENT TYPES:
/// - Initiated: Payment session created with gateway
/// - WebhookReceived: Gateway notification processed
/// - WebhookDuplicate: Duplicate webhook ignored
/// - Verified: Status verified with gateway API
/// - Confirmed: Payment successful
/// - Failed: Payment failed
/// - Cancelled: Payment cancelled by user/system
/// </summary>
public class PaymentEvent : BaseEntity
{
    public Guid PaymentId { get; set; }
    public virtual Payment Payment { get; set; } = null!;
    
    /// <summary>
    /// Type of event: Initiated, Confirmed, Failed, Cancelled, WebhookReceived, WebhookDuplicate, Verified
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique identifier from payment gateway
    /// Used to detect duplicate webhooks
    /// Format: gateway_event_id or external_transaction_id
    /// </summary>
    public string? GatewayEventId { get; set; }
    
    /// <summary>
    /// JSON payload from gateway webhook/API
    /// </summary>
    public string EventData { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether this event was successfully processed (true) or duplicate/ignored (false)
    /// </summary>
    public bool IsProcessed { get; set; } = true;
}
