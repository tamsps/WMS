namespace WMS.Products.API.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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

public class CreateProductDto
{
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

public class UpdateProductDto
{
    public Guid Id { get; set; }
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
