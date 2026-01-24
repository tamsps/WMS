using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetLocationByCode;

public class GetLocationByCodeQueryHandler : IRequestHandler<GetLocationByCodeQuery, Result<LocationDto>>
{
    private readonly WMSDbContext _context;

    public GetLocationByCodeQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LocationDto>> Handle(GetLocationByCodeQuery request, CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Code == request.Code, cancellationToken);

        if (location == null)
        {
            return Result<LocationDto>.Failure($"Location with code '{request.Code}' not found");
        }

        return Result<LocationDto>.Success(LocationMapper.MapToDto(location));
    }
}
