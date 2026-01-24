using WMS.Domain.Entities;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Mappers;

public static class ProductMapper
{
    public static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            UOM = product.UOM,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            ReorderLevel = product.ReorderLevel,
            MaxStockLevel = product.MaxStockLevel,
            Status = product.Status.ToString(),
            Barcode = product.Barcode,
            CreatedAt = product.CreatedAt
        };
    }
}
