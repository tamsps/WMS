using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inventory.API.Application.Mappers;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryTransactions;

public class GetInventoryTransactionsQueryHandler : IRequestHandler<GetInventoryTransactionsQuery, Result<PagedResult<InventoryTransactionDto>>>
{
    private readonly WMSDbContext _context;

    public GetInventoryTransactionsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InventoryTransactionDto>>> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .Include(t => t.Location)
            .AsQueryable();

        if (request.ProductId.HasValue)
        {
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(t => t.LocationId == request.LocationId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<InventoryTransactionDto>
        {
            Items = transactions.Select(InventoryMapper.MapToTransactionDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<InventoryTransactionDto>>.Success(result);
    }
}
