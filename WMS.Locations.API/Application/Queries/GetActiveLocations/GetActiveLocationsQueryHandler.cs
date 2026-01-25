using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetActiveLocations;

/// <summary>
/// Handler for retrieving only active locations
/// Commonly used by transaction services to ensure only active locations are available
/// </summary>
public class GetActiveLocationsQueryHandler : IRequestHandler<GetActiveLocationsQuery, Result<PagedResult<LocationDto>>>
{
    private readonly WMSDbContext _context;

    public GetActiveLocationsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<LocationDto>>> Handle(GetActiveLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Locations
            .Where(l => l.IsActive)
            .AsQueryable();

        // Filter by location type if specified
        if (!string.IsNullOrWhiteSpace(request.LocationType))
        {
            query = query.Where(l => l.LocationType == request.LocationType);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(l =>
                l.Code.Contains(request.SearchTerm) ||
                l.Name.Contains(request.SearchTerm) ||
                (l.Description != null && l.Description.Contains(request.SearchTerm)));
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
