using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Outbound Entity - Shipping confirmation process
/// Decreases inventory and prevents negative stock
/// </summary>
public class Outbound : BaseEntity
{
    public string OutboundNumber { get; set; } = string.Empty;
    public string? OrderNumber { get; set; } // Sales Order, Transfer Order, etc.
    public OutboundStatus Status { get; set; } = OutboundStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ShipDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Payment reference for shipment gating
    public Guid? PaymentId { get; set; }
    public virtual Payment? Payment { get; set; }
    
    // Delivery reference
    public Guid? DeliveryId { get; set; }
    public virtual Delivery? Delivery { get; set; }
    
    // Navigation properties
    public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();
}

public class OutboundItem : BaseEntity
{
    public Guid OutboundId { get; set; }
    public virtual Outbound Outbound { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public decimal OrderedQuantity { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
}
