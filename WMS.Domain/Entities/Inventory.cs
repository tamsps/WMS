using WMS.Domain.Common;

namespace WMS.Domain.Entities;

/// <summary>
/// Inventory Entity - Real-time stock levels by SKU and location
/// Derived exclusively from validated inbound and outbound transactions
/// 
/// CRITICAL DATA CONSISTENCY REQUIREMENTS:
/// 
/// 1. CONCURRENCY CONTROL:
///    - Uses RowVersion (optimistic concurrency) to prevent lost updates
///    - Multiple users can pick/receive simultaneously without blocking
///    - DbUpdateConcurrencyException thrown if data changed between read and update
///    - Application must retry with fresh data on concurrency conflicts
/// 
/// 2. ATOMIC TRANSACTIONS:
///    - All inventory changes wrapped in database transactions
///    - Inbound: QuantityOnHand increased ONLY after successful receive
///    - Outbound Pick: QuantityReserved increased atomically
///    - Outbound Ship: QuantityReserved and QuantityOnHand decreased atomically
///    - Transaction rollback ensures no partial updates
/// 
/// 3. NEGATIVE STOCK PREVENTION:
///    - QuantityAvailable = QuantityOnHand - QuantityReserved
///    - Pick operations validate: QuantityAvailable >= PickQuantity
///    - Ship operations validate: QuantityReserved >= ShipQuantity
///    - Database constraints prevent negative values
/// 
/// 4. AUDIT TRAIL:
///    - Every inventory change creates InventoryTransaction record
///    - BalanceBefore and BalanceAfter tracked for reconciliation
///    - ReferenceId links to source document (Inbound/Outbound)
///    - Complete audit trail for financial/operational reconciliation
/// 
/// 5. UNIQUENESS:
///    - Unique index on (ProductId, LocationId)
///    - One inventory record per product per location
///    - Enforces single source of truth for stock levels
/// 
/// 6. REAL-TIME ACCURACY:
///    - No scheduled batch updates
///    - Changes applied immediately in transaction
///    - Queries always return current committed data
/// </summary>
public class Inventory : BaseEntity
{
    /// <summary>
    /// Product being tracked
    /// </summary>
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Location where inventory is stored
    /// </summary>
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    /// <summary>
    /// Total physical quantity in location
    /// Increased by: Inbound receipts, Returns
    /// Decreased by: Outbound shipments
    /// MUST NEVER be negative
    /// </summary>
    public decimal QuantityOnHand { get; set; }
    
    /// <summary>
    /// Quantity allocated to outbound orders (not yet shipped)
    /// Increased by: Pick operations
    /// Decreased by: Ship operations, Pick cancellations
    /// MUST NEVER exceed QuantityOnHand
    /// </summary>
    public decimal QuantityReserved { get; set; }
    
    /// <summary>
    /// Quantity available for new orders
    /// Calculated: QuantityOnHand - QuantityReserved
    /// Used for: Pick validation, Available-to-Promise (ATP)
    /// </summary>
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    
    /// <summary>
    /// Last time inventory was updated
    /// Used for: Aging analysis, reconciliation scheduling
    /// </summary>
    public DateTime LastStockDate { get; set; } = DateTime.UtcNow;
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// Validate inventory state consistency
    /// </summary>
    public bool IsValid()
    {
        return QuantityOnHand >= 0 && 
               QuantityReserved >= 0 && 
               QuantityReserved <= QuantityOnHand;
    }
}
