using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Product (SKU) Entity - Immutable product identifier
/// Manages master data for all warehouse operations
/// </summary>
public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string UOM { get; set; } = string.Empty; // Unit of Measure
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    
    // Navigation properties
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
