using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Queries.GetAllInbounds;

public class GetAllInboundsQueryHandler : IRequestHandler<GetAllInboundsQuery, Result<PagedResult<InboundDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllInboundsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<InboundDto>>> Handle(GetAllInboundsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InboundStatus>(request.Status, out var inboundStatus))
        {
            query = query.Where(i => i.Status == inboundStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var inbounds = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<InboundDto>
        {
            Items = inbounds.Select(InboundMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<InboundDto>>.Success(result);
    }
}
