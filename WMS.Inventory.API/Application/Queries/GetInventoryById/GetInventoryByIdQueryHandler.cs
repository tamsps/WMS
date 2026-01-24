using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inventory.API.Application.Mappers;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryById;

public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, Result<InventoryDto>>
{
    private readonly WMSDbContext _context;

    public GetInventoryByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InventoryDto>> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Location)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inventory == null)
        {
            return Result<InventoryDto>.Failure("Inventory not found");
        }

        return Result<InventoryDto>.Success(InventoryMapper.MapToDto(inventory));
    }
}
