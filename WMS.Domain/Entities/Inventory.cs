using WMS.Domain.Common;

namespace WMS.Domain.Entities;

/// <summary>
/// Inventory Entity - Real-time stock levels by SKU and location
/// Derived exclusively from validated inbound and outbound transactions
/// </summary>
public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    
    public DateTime LastStockDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
