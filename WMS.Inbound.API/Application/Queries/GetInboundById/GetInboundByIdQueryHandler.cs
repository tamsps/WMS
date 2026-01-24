using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Queries.GetInboundById;

public class GetInboundByIdQueryHandler : IRequestHandler<GetInboundByIdQuery, Result<InboundDto>>
{
    private readonly WMSDbContext _context;

    public GetInboundByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InboundDto>> Handle(GetInboundByIdQuery request, CancellationToken cancellationToken)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        return Result<InboundDto>.Success(InboundMapper.MapToDto(inbound));
    }
}
