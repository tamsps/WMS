using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Queries.GetAllOutbounds;

public class GetAllOutboundsQueryHandler : IRequestHandler<GetAllOutboundsQuery, Result<PagedResult<OutboundDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllOutboundsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<OutboundDto>>> Handle(GetAllOutboundsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OutboundStatus>(request.Status, out var outboundStatus))
        {
            query = query.Where(o => o.Status == outboundStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var outbounds = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<OutboundDto>
        {
            Items = outbounds.Select(OutboundMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<OutboundDto>>.Success(result);
    }
}
