using MediatR;
using WMS.Locations.API.Common.Models;

namespace WMS.Locations.API.Application.Queries.CheckLocationCapacity;

/// <summary>
/// Query to check if a location has sufficient capacity for a given amount
/// </summary>
public class CheckLocationCapacityQuery : IRequest<Result<LocationCapacityResponse>>
{
    public Guid LocationId { get; set; }
    public decimal RequiredCapacity { get; set; }
}

/// <summary>
/// Response for capacity check query
/// </summary>
public class LocationCapacityResponse
{
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public decimal TotalCapacity { get; set; }
    public decimal CurrentOccupancy { get; set; }
    public decimal AvailableCapacity { get; set; }
    public decimal RequiredCapacity { get; set; }
    public bool HasSufficientCapacity { get; set; }
    public decimal ShortageAmount { get; set; }
}
