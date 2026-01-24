using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inventory.API.Application.Mappers;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetAllInventory;

public class GetAllInventoryQueryHandler : IRequestHandler<GetAllInventoryQuery, Result<PagedResult<InventoryDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllInventoryQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InventoryDto>>> Handle(GetAllInventoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Location)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var inventories = await query
            .OrderBy(i => i.Product.SKU)
            .ThenBy(i => i.Location.Code)
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
