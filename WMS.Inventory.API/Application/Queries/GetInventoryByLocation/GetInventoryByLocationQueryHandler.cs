using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inventory.API.Application.Mappers;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryByLocation;

public class GetInventoryByLocationQueryHandler : IRequestHandler<GetInventoryByLocationQuery, Result<PagedResult<InventoryDto>>>
{
    private readonly WMSDbContext _context;

    public GetInventoryByLocationQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InventoryDto>>> Handle(GetInventoryByLocationQuery request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations.FindAsync(new object[] { request.LocationId }, cancellationToken);
        if (location == null)
        {
            return Result<PagedResult<InventoryDto>>.Failure("Location not found");
        }

        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Location)
            .Where(i => i.LocationId == request.LocationId);

        var totalCount = await query.CountAsync(cancellationToken);

        var inventories = await query
            .OrderBy(i => i.Product.SKU)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<InventoryDto>
        {
            Items = inventories.Select(InventoryMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<InventoryDto>>.Success(result);
    }
}
