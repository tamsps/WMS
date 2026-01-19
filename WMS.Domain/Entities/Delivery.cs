using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Delivery Entity - Physical shipment management
/// Inventory deducted at outbound confirmation, not at delivery completion
/// </summary>
public class Delivery : BaseEntity
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public Guid OutboundId { get; set; }
    public virtual Outbound Outbound { get; set; } = null!;
    
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    
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
    
    // Return handling
    public bool IsReturn { get; set; }
    public Guid? ReturnInboundId { get; set; }
    
    // Navigation properties
    public virtual ICollection<DeliveryEvent> DeliveryEvents { get; set; } = new List<DeliveryEvent>();
}

public class DeliveryEvent : BaseEntity
{
    public Guid DeliveryId { get; set; }
    public virtual Delivery Delivery { get; set; } = null!;
    
    public string EventType { get; set; } = string.Empty; // PickedUp, InTransit, Delivered, Failed, Returned
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? Notes { get; set; }
}
