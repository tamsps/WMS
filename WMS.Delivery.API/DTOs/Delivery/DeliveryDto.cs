namespace WMS.Delivery.API.DTOs.Delivery;

public class DeliveryDto
{
    public Guid Id { get; set; }
    public string DeliveryNumber { get; set; } = string.Empty;
    public Guid OutboundId { get; set; }
    public string? OutboundNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? FailureReason { get; set; }
    public bool IsReturn { get; set; }
    public List<DeliveryEventDto> Events { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class DeliveryEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class CreateDeliveryDto
{
    public Guid OutboundId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? DeliveryNotes { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EventLocation { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Delivery webhook DTO for processing callbacks from external delivery partners
/// 
/// IDEMPOTENCY:
/// - PartnerEventId must be unique for each webhook
/// - Same PartnerEventId received multiple times = duplicate (safe to ignore)
/// - TrackingNumber identifies the delivery
/// </summary>
public class DeliveryWebhookDto
{
    /// <summary>
    /// Tracking number to identify the delivery
    /// Required to find the delivery record
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique event ID from delivery partner
    /// Used for idempotency - prevents duplicate processing
    /// Examples: fedex_event_123, ups_webhook_456, dhl_callback_789
    /// </summary>
    public string PartnerEventId { get; set; } = string.Empty;
    
    /// <summary>
    /// Delivery status from webhook: InTransit, Delivered, Failed, etc.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Current location of delivery
    /// </summary>
    public string? CurrentLocation { get; set; }
    
    /// <summary>
    /// Event timestamp from delivery partner
    /// </summary>
    public DateTime? EventTimestamp { get; set; }
    
    /// <summary>
    /// Full JSON payload from partner (for audit/debugging)
    /// </summary>
    public string EventData { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional notes from delivery partner
    /// </summary>
    public string? Notes { get; set; }
}

public class CompleteDeliveryDto
{
    public Guid DeliveryId { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
}

public class FailDeliveryDto
{
    public Guid DeliveryId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AddDeliveryEventDto
{
    public Guid DeliveryId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
}
