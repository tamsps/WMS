using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Outbound Entity - Shipping confirmation process
/// Decreases inventory and prevents negative stock
/// 
/// Outbound Processing Lifecycle:
/// 1. Pending - Outbound created, awaiting picking
/// 2. Picking - Items are being picked from locations
/// 3. Picked - All items picked, ready for packing
/// 4. Packed - Items packed, ready to ship
/// 5. Shipped - Inventory deducted, ready for delivery
/// 6. Cancelled - Outbound cancelled
/// 
/// Payment Integration:
/// - Prepaid orders MUST have confirmed payment before shipping
/// - COD (Cash On Delivery) orders can ship without payment confirmation
/// - Postpaid orders can ship without payment confirmation
/// - Payment confirmation is validated during ship operation
/// 
/// Inventory Management:
/// - Inventory is reserved at Pick operation
/// - Inventory is deducted at Ship operation (atomic transaction)
/// - Negative stock is prevented
/// - Location capacity is released when shipped
/// </summary>
public class Outbound : BaseEntity
{
    /// <summary>
    /// Unique outbound number generated automatically
    /// Format: OUT-YYYYMMDD-####
    /// </summary>
    public string OutboundNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// External order number (e.g., Sales Order, Transfer Order)
    /// </summary>
    public string? OrderNumber { get; set; }
    
    /// <summary>
    /// Current status of the outbound shipment
    /// Controls which operations can be performed
    /// </summary>
    public OutboundStatus Status { get; set; } = OutboundStatus.Pending;
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Actual date when goods were shipped
    /// Set when status changes to Shipped
    /// </summary>
    public DateTime? ShipDate { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    /// <summary>
    /// Payment reference for shipment gating
    /// Required for prepaid orders - must be confirmed before shipping
    /// Optional for COD/Postpaid orders
    /// </summary>
    public Guid? PaymentId { get; set; }
    public virtual Payment? Payment { get; set; }
    
    /// <summary>
    /// Delivery reference
    /// Created after outbound is shipped
    /// </summary>
    public Guid? DeliveryId { get; set; }
    public virtual Delivery? Delivery { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Line items in this outbound order
    /// Each item tracks ordered, picked, and shipped quantities
    /// </summary>
    public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();
}

/// <summary>
/// Outbound Item Entity - Individual line item in an outbound order
/// Tracks quantity progression: Ordered ? Picked ? Shipped
/// </summary>
public class OutboundItem : BaseEntity
{
    public Guid OutboundId { get; set; }
    public virtual Outbound Outbound { get; set; } = null!;
    
    /// <summary>
    /// Product being shipped
    /// </summary>
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Source location for this product
    /// </summary>
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    /// <summary>
    /// Quantity ordered by customer
    /// </summary>
    public decimal OrderedQuantity { get; set; }
    
    /// <summary>
    /// Quantity actually picked from warehouse
    /// Inventory is reserved when picked
    /// </summary>
    public decimal PickedQuantity { get; set; }
    
    /// <summary>
    /// Quantity shipped to customer
    /// Inventory is deducted when shipped
    /// Usually equals PickedQuantity
    /// </summary>
    public decimal ShippedQuantity { get; set; }
    
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Check if item is fully picked
    /// </summary>
    public bool IsFullyPicked() => PickedQuantity >= OrderedQuantity;
    
    /// <summary>
    /// Check if item is short-picked (partially picked)
    /// </summary>
    public bool IsShortPicked() => PickedQuantity > 0 && PickedQuantity < OrderedQuantity;
}
