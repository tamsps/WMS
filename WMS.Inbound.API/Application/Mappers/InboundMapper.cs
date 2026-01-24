using WMS.Domain.Entities;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Mappers;

public static class InboundMapper
{
    public static InboundDto MapToDto(WMS.Domain.Entities.Inbound inbound)
    {
        return new InboundDto
        {
            Id = inbound.Id,
            InboundNumber = inbound.InboundNumber,
            ReferenceNumber = inbound.ReferenceNumber,
            Status = inbound.Status.ToString(),
            ExpectedDate = inbound.ExpectedDate,
            ReceivedDate = inbound.ReceivedDate,
            SupplierName = inbound.SupplierName,
            SupplierCode = inbound.SupplierCode,
            Notes = inbound.Notes,
            Items = inbound.InboundItems.Select(ii => new InboundItemDto
            {
                Id = ii.Id,
                ProductId = ii.ProductId,
                ProductSKU = ii.Product.SKU,
                ProductName = ii.Product.Name,
                LocationId = ii.LocationId,
                LocationCode = ii.Location.Code,
                LocationName = ii.Location.Name,
                ExpectedQuantity = ii.ExpectedQuantity,
                ReceivedQuantity = ii.ReceivedQuantity,
                DamagedQuantity = ii.DamagedQuantity,
                LotNumber = ii.LotNumber,
                ExpiryDate = ii.ExpiryDate,
                Notes = ii.Notes
            }).ToList(),
            CreatedAt = inbound.CreatedAt
        };
    }
}
