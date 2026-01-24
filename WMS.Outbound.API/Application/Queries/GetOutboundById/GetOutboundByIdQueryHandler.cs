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

        return Result<OutboundDto>.Success(OutboundMapper.MapToDto(outbound));
    }
}
