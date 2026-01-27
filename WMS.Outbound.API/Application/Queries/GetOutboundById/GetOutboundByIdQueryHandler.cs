using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Queries.GetOutboundById;

public class GetOutboundByIdQueryHandler : IRequestHandler<GetOutboundByIdQuery, Result<OutboundDto>>
{
    private readonly WMSDbContext _context;

    public GetOutboundByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OutboundDto>> Handle(GetOutboundByIdQuery request, CancellationToken cancellationToken)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        // Get product and location IDs for inventory lookup
        var productIds = outbound.OutboundItems.Select(oi => oi.ProductId).Distinct().ToList();
        var locationIds = outbound.OutboundItems.Select(oi => oi.LocationId).Distinct().ToList();

        // Query inventories - filter by product and location IDs separately (EF Core can translate this)
        var inventories = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId) && locationIds.Contains(i.LocationId))
            .ToListAsync(cancellationToken);

        // Map to DTO with available quantities
        var dto = new OutboundDto
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
                // Find matching inventory for this specific product-location combination
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

        return Result<OutboundDto>.Success(dto);
    }
}
