using MediatR;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetActiveLocations;

/// <summary>
/// Query to retrieve only active locations (convenience query for transaction operations)
/// </summary>
public class GetActiveLocationsQuery : IRequest<Result<PagedResult<LocationDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? LocationType { get; set; }
}
