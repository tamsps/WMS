using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inventory.API.Application.Mappers;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryByProduct;

public class GetInventoryByProductQueryHandler : IRequestHandler<GetInventoryByProductQuery, Result<InventoryLevelDto>>
{
    private readonly WMSDbContext _context;

    public GetInventoryByProductQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InventoryLevelDto>> Handle(GetInventoryByProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
        if (product == null)
        {
            return Result<InventoryLevelDto>.Failure("Product not found");
        }

        var inventories = await _context.Inventories
            .Include(i => i.Location)
            .Where(i => i.ProductId == request.ProductId)
            .ToListAsync(cancellationToken);

        var dto = InventoryMapper.MapToInventoryLevelDto(product, inventories);

        return Result<InventoryLevelDto>.Success(dto);
    }
}
