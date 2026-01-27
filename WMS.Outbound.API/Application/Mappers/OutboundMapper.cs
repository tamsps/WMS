using WMS.Domain.Entities;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Mappers;

public static class OutboundMapper
{
    public static OutboundDto MapToDto(WMS.Domain.Entities.Outbound outbound)
    {
        return new OutboundDto
        {
            Id = outbound.Id,
            OutboundNumber = outbound.OutboundNumber,
            OrderNumber = outbound.OrderNumber,
            Status = outbound.Status.ToString(),
            OrderDate = outbound.OrderDate,
            ShipDate = outbound.ShipDate,
            CustomerName = outbound.CustomerName,
            CustomerCode = outbound.CustomerCode,
            ShippingAddress = outbound.ShippingAddress,
            Notes = outbound.Notes,
            PaymentId = outbound.PaymentId,
            Items = outbound.OutboundItems.Select(oi => new OutboundItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductSKU = oi.Product.SKU,
                ProductName = oi.Product.Name,
                LocationId = oi.LocationId,
                LocationCode = oi.Location.Code,
                LocationName = oi.Location.Name,
                OrderedQuantity = oi.OrderedQuantity,
                PickedQuantity = oi.PickedQuantity,
                ShippedQuantity = oi.ShippedQuantity,
                AvailableQuantity = 0, // Will be populated by query handler with actual inventory data
                LotNumber = oi.LotNumber,
                SerialNumber = oi.SerialNumber,
                Notes = oi.Notes,
                UOM = oi.Product.UOM ?? "EA"
            }).ToList(),
            CreatedAt = outbound.CreatedAt
        };
    }

    public static OutboundDto MapToDto(WMS.Domain.Entities.Outbound outbound, IEnumerable<Inventory> inventories)
    {
        return new OutboundDto
        {
            Id = outbound.Id,
            OutboundNumber = outbound.OutboundNumber,
            OrderNumber = outbound.OrderNumber,
            Status = outbound.Status.ToString(),
            OrderDate = outbound.OrderDate,
            ShipDate = outbound.ShipDate,
            CustomerName = outbound.CustomerName,
            CustomerCode = outbound.CustomerCode,
            ShippingAddress = outbound.ShippingAddress,
            Notes = outbound.Notes,
            PaymentId = outbound.PaymentId,
            Items = outbound.OutboundItems.Select(oi =>
            {
                var inventory = inventories.FirstOrDefault(i => i.ProductId == oi.ProductId && i.LocationId == oi.LocationId);
                return new OutboundItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductSKU = oi.Product.SKU,
                    ProductName = oi.Product.Name,
                    LocationId = oi.LocationId,
                    LocationCode = oi.Location.Code,
                    LocationName = oi.Location.Name,
                    OrderedQuantity = oi.OrderedQuantity,
                    PickedQuantity = oi.PickedQuantity,
                    ShippedQuantity = oi.ShippedQuantity,
                    AvailableQuantity = inventory?.QuantityAvailable ?? 0,
                    LotNumber = oi.LotNumber,
                    SerialNumber = oi.SerialNumber,
                    Notes = oi.Notes,
                    UOM = oi.Product.UOM ?? "EA"
                };
            }).ToList(),
            CreatedAt = outbound.CreatedAt
        };
    }
}
