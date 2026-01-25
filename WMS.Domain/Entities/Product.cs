using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Product (SKU) Entity - Immutable product identifier
/// Manages master data for all warehouse operations
/// 
/// Product Lifecycle Management:
/// - Products are created with a unique, immutable SKU
/// - Products can be activated or deactivated via Status property
/// - Only active products can participate in new warehouse transactions
/// - Historical transactions remain valid even if a product is deactivated
/// - SKU cannot be changed once created (enforced by excluding from UpdateProductDto)
/// - SKU uniqueness is enforced by database unique index
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Stock Keeping Unit - Unique, immutable identifier for the product
    /// Cannot be changed after product creation
    /// </summary>
    public string SKU { get; set; } = string.Empty;
    
    /// <summary>
    /// Product display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Product status - Active products can be used in transactions, Inactive products cannot
    /// Historical transactions with inactive products remain valid
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    
    /// <summary>
    /// Unit of Measure (e.g., PCS, KG, BOX)
    /// </summary>
    public string UOM { get; set; } = string.Empty;
    
    /// <summary>
    /// Product weight in kilograms
    /// </summary>
    public decimal Weight { get; set; }
    
    /// <summary>
    /// Product length in centimeters
    /// </summary>
    public decimal Length { get; set; }
    
    /// <summary>
    /// Product width in centimeters
    /// </summary>
    public decimal Width { get; set; }
    
    /// <summary>
    /// Product height in centimeters
    /// </summary>
    public decimal Height { get; set; }
    
    /// <summary>
    /// Product barcode (optional)
    /// </summary>
    public string? Barcode { get; set; }
    
    /// <summary>
    /// Product category (optional)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Minimum stock level that triggers reorder alerts (optional)
    /// </summary>
    public decimal? ReorderLevel { get; set; }
    
    /// <summary>
    /// Maximum recommended stock level (optional)
    /// </summary>
    public decimal? MaxStockLevel { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// All inventory transactions involving this product
    /// Maintained even after product deactivation for historical accuracy
    /// </summary>
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    
    /// <summary>
    /// Current inventory records for this product across all locations
    /// </summary>
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
