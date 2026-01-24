using WMS.Domain.Entities;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Mappers;

public static class InventoryMapper
{
    public static InventoryDto MapToDto(WMS.Domain.Entities.Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductSKU = inventory.Product.SKU,
            ProductName = inventory.Product.Name,
            LocationId = inventory.LocationId,
            LocationCode = inventory.Location.Code,
            LocationName = inventory.Location.Name,
            QuantityOnHand = inventory.QuantityOnHand,
            QuantityReserved = inventory.QuantityReserved,
            QuantityAvailable = inventory.QuantityAvailable,
            LastStockDate = inventory.LastStockDate
        };
    }

    public static InventoryTransactionDto MapToTransactionDto(WMS.Domain.Entities.InventoryTransaction transaction)
    {
        return new InventoryTransactionDto
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            TransactionType = transaction.TransactionType.ToString(),
            ProductId = transaction.ProductId,
            ProductSKU = transaction.Product.SKU,
            ProductName = transaction.Product.Name,
            LocationId = transaction.LocationId,
            LocationCode = transaction.Location.Code,
            Quantity = transaction.Quantity,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            ReferenceType = transaction.ReferenceType,
            ReferenceNumber = transaction.ReferenceNumber,
            CreatedAt = transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy
        };
    }

    public static InventoryLevelDto MapToInventoryLevelDto(WMS.Domain.Entities.Product product, List<WMS.Domain.Entities.Inventory> inventories)
    {
        return new InventoryLevelDto
        {
            ProductId = product.Id,
            ProductSKU = product.SKU,
            ProductName = product.Name,
            TotalQuantity = inventories.Sum(i => i.QuantityOnHand),
            TotalReserved = inventories.Sum(i => i.QuantityReserved),
            TotalAvailable = inventories.Sum(i => i.QuantityAvailable),
            LocationInventories = inventories.Select(i => new LocationInventoryDto
            {
                LocationId = i.LocationId,
                LocationCode = i.Location.Code,
                Quantity = i.QuantityOnHand,
                Reserved = i.QuantityReserved,
                Available = i.QuantityAvailable
            }).ToList()
        };
    }
}
