using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Payment Entity - Operational payment state management
/// Responsible for payment state and shipment gating, not financial settlement
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
    
    // External payment gateway reference
    public string? ExternalPaymentId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentMethod { get; set; } // Credit Card, Bank Transfer, Cash, etc.
    
    public DateTime? PaymentDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    
    // Audit trail for payment events
    public virtual ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
}

public class PaymentEvent : BaseEntity
{
    public Guid PaymentId { get; set; }
    public virtual Payment Payment { get; set; } = null!;
    
    public string EventType { get; set; } = string.Empty; // Initiated, Confirmed, Failed, Cancelled
    public string EventData { get; set; } = string.Empty; // JSON data from gateway
    public string? Notes { get; set; }
}
