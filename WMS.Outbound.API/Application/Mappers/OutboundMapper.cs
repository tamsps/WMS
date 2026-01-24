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
                OrderedQuantity = oi.OrderedQuantity,
                PickedQuantity = oi.PickedQuantity,
                ShippedQuantity = oi.ShippedQuantity,
                LotNumber = oi.LotNumber,
                SerialNumber = oi.SerialNumber,
                Notes = oi.Notes
            }).ToList(),
            CreatedAt = outbound.CreatedAt
        };
    }
}
