using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetAllLocations;

public class GetAllLocationsQueryHandler : IRequestHandler<GetAllLocationsQuery, Result<PagedResult<LocationDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllLocationsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<LocationDto>>> Handle(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(l =>
                l.Code.Contains(request.SearchTerm) ||
                l.Name.Contains(request.SearchTerm) ||
                l.Zone.Contains(request.SearchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(l => l.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var locations = await query
            .OrderBy(l => l.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<LocationDto>
        {
            Items = locations.Select(LocationMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<LocationDto>>.Success(result);
    }
}
