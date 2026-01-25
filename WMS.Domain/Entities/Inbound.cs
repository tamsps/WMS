using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Inbound Entity - Receiving goods process
/// Handles the receipt of goods and increases inventory through atomic transactions
/// 
/// Inbound Processing Lifecycle:
/// 1. Pending - Inbound created, awaiting receipt
/// 2. Received - Goods have been received and inventory updated
/// 3. PutAway - Goods have been moved to storage locations
/// 4. Completed - Inbound process completed
/// 5. Cancelled - Inbound cancelled (only from Pending status)
/// 
/// Atomic Transaction Requirements:
/// - All inbound operations must execute as atomic transactions
/// - If any part of the receive operation fails, entire transaction is rolled back
/// - Inventory updates must be consistent with received quantities
/// - Location capacity must be validated and enforced
/// - Product active status must be validated before receiving
/// </summary>
public class Inbound : BaseEntity
{
    /// <summary>
    /// Unique inbound number generated automatically
    /// Format: IB-YYYYMMDD-####
    /// </summary>
    public string InboundNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// External reference number (e.g., PO Number, Transfer Order)
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Current status of the inbound shipment
    /// Controls which operations can be performed
    /// </summary>
    public InboundStatus Status { get; set; } = InboundStatus.Pending;
    
    /// <summary>
    /// Expected delivery date
    /// Used for planning and scheduling
    /// </summary>
    public DateTime ExpectedDate { get; set; }
    
    /// <summary>
    /// Actual date when goods were received
    /// Set when status changes to Received
    /// </summary>
    public DateTime? ReceivedDate { get; set; }
    
    /// <summary>
    /// Supplier/vendor name
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;
    
    /// <summary>
    /// Supplier/vendor code (optional)
    /// </summary>
    public string? SupplierCode { get; set; }
    
    /// <summary>
    /// General notes about the inbound shipment
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Line items in this inbound shipment
    /// Each item represents expected and received quantities for a product at a location
    /// </summary>
    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();
}

/// <summary>
/// Inbound Item Entity - Individual line item in an inbound shipment
/// Tracks expected vs received quantities with quality control data
/// </summary>
public class InboundItem : BaseEntity
{
    public Guid InboundId { get; set; }
    public virtual Inbound Inbound { get; set; } = null!;
    
    /// <summary>
    /// Product being received
    /// Must be an active product
    /// </summary>
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Destination location for this product
    /// Must be an active location with sufficient capacity
    /// </summary>
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    /// <summary>
    /// Expected quantity to be received
    /// From purchase order or transfer order
    /// </summary>
    public decimal ExpectedQuantity { get; set; }
    
    /// <summary>
    /// Actual quantity received (including damaged)
    /// Updated during receive operation
    /// </summary>
    public decimal ReceivedQuantity { get; set; }
    
    /// <summary>
    /// Quantity of damaged/defective items
    /// Does not increase inventory
    /// </summary>
    public decimal? DamagedQuantity { get; set; }
    
    /// <summary>
    /// Lot number for traceability (optional)
    /// </summary>
    public string? LotNumber { get; set; }
    
    /// <summary>
    /// Expiry date for perishable items (optional)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Item-specific notes (e.g., quality issues, special handling)
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Calculate good quantity (received minus damaged)
    /// This is the amount that increases inventory
    /// </summary>
    public decimal GetGoodQuantity() => ReceivedQuantity - (DamagedQuantity ?? 0);
    
    /// <summary>
    /// Check if item is fully received
    /// </summary>
    public bool IsFullyReceived() => ReceivedQuantity >= ExpectedQuantity;
    
    /// <summary>
    /// Check if item is partially received
    /// </summary>
    public bool IsPartiallyReceived() => ReceivedQuantity > 0 && ReceivedQuantity < ExpectedQuantity;
}
