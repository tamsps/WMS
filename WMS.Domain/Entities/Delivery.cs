using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Delivery Entity - Last-mile delivery tracking
/// Manages delivery status separate from warehouse operations
/// 
/// ASYNCHRONOUS INTEGRATION:
/// - Delivery updates arrive from external partners via webhooks
/// - Updates processed independently from inventory transactions
/// - Failed deliveries may trigger return inbound (separate flow)
/// </summary>
public class Delivery : BaseEntity
{
    public string DeliveryNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference to outbound shipment
    /// </summary>
    public Guid OutboundId { get; set; }
    public virtual Outbound Outbound { get; set; } = null!;
    
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    
    /// <summary>
    /// Shipping address details
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    
    /// <summary>
    /// Delivery partner/carrier (FedEx, UPS, DHL, etc.)
    /// </summary>
    public string? Carrier { get; set; }
    
    /// <summary>
    /// Tracking number from delivery partner
    /// Used to identify delivery in webhook callbacks
    /// </summary>
    public string? TrackingNumber { get; set; }
    
    /// <summary>
    /// Vehicle/truck number
    /// </summary>
    public string? VehicleNumber { get; set; }
    
    /// <summary>
    /// Driver name
    /// </summary>
    public string? DriverName { get; set; }
    
    /// <summary>
    /// Driver contact phone
    /// </summary>
    public string? DriverPhone { get; set; }
    
    public DateTime? PickupDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    
    /// <summary>
    /// Person who received the delivery
    /// </summary>
    public string? ReceivedBy { get; set; }
    
    public string? DeliveryNotes { get; set; }
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Indicates if this is a return delivery
    /// </summary>
    public bool IsReturn { get; set; }
    
    /// <summary>
    /// Delivery event history (includes webhook events)
    /// </summary>
    public virtual ICollection<DeliveryEvent> DeliveryEvents { get; set; } = new List<DeliveryEvent>();
}

/// <summary>
/// Delivery Event Entity - Tracks delivery status changes and partner webhooks
/// 
/// WEBHOOK IDEMPOTENCY:
/// - PartnerEventId uniquely identifies each webhook
/// - Duplicate webhooks with same PartnerEventId are ignored
/// - All webhook attempts are logged
/// </summary>
public class DeliveryEvent : BaseEntity
{
    public Guid DeliveryId { get; set; }
    public virtual Delivery Delivery { get; set; } = null!;
    
    /// <summary>
    /// Event type: InTransit, OutForDelivery, Delivered, Failed, etc.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique event ID from delivery partner
    /// Used to detect duplicate webhooks
    /// Examples: fedex_event_123, ups_webhook_456
    /// </summary>
    public string? PartnerEventId { get; set; }
    
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Location where event occurred
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// JSON payload from partner webhook (for audit)
    /// </summary>
    public string? EventData { get; set; }
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// Whether this event was successfully processed (true) or duplicate/ignored (false)
    /// </summary>
    public bool IsProcessed { get; set; } = true;
}
