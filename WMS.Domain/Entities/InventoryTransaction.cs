using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Inventory Transaction Entity - Audit trail for all inventory movements
/// Records all changes to inventory with full traceability
/// </summary>
public class InventoryTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public decimal Quantity { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    
    // Reference to source transaction
    public Guid? ReferenceId { get; set; } // InboundId, OutboundId, etc.
    public string? ReferenceType { get; set; } // Inbound, Outbound, Adjustment, Return
    public string? ReferenceNumber { get; set; }
    
    public string? Notes { get; set; }
}
