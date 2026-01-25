namespace WMS.Products.API.DTOs.Product;

/// <summary>
/// Data transfer object for product information
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Stock Keeping Unit - Unique, immutable identifier
    /// </summary>
    public string SKU { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Product status (Active/Inactive)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    public string UOM { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Stock Keeping Unit - Must be unique across all products
    /// Once set, it cannot be changed
    /// </summary>
    public string SKU { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing product
/// Note: SKU is not included as it is immutable and cannot be changed
/// </summary>
public class UpdateProductDto
{
    public Guid Id { get; set; }
    
    // SKU is intentionally excluded - it is immutable and cannot be updated
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
}
