using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Locations.API.Common.Models;

namespace WMS.Locations.API.Application.Queries.CheckLocationCapacity;

/// <summary>
/// Handler for checking location capacity availability
/// </summary>
public class CheckLocationCapacityQueryHandler : IRequestHandler<CheckLocationCapacityQuery, Result<LocationCapacityResponse>>
{
    private readonly IRepository<Location> _locationRepository;

    public CheckLocationCapacityQueryHandler(IRepository<Location> locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<LocationCapacityResponse>> Handle(CheckLocationCapacityQuery request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        
        if (location == null)
        {
            return Result<LocationCapacityResponse>.Failure("Location not found");
        }

        if (!location.IsActive)
        {
            return Result<LocationCapacityResponse>.Failure("Location is not active");
        }

        var availableCapacity = location.GetAvailableCapacity();
        var hasSufficientCapacity = location.HasCapacityFor(request.RequiredCapacity);
        var shortage = hasSufficientCapacity ? 0 : request.RequiredCapacity - availableCapacity;

        var response = new LocationCapacityResponse
        {
            LocationId = location.Id,
            LocationCode = location.Code,
            LocationName = location.Name,
            TotalCapacity = location.Capacity,
            CurrentOccupancy = location.CurrentOccupancy,
            AvailableCapacity = availableCapacity,
            RequiredCapacity = request.RequiredCapacity,
            HasSufficientCapacity = hasSufficientCapacity,
            ShortageAmount = shortage
        };

        return Result<LocationCapacityResponse>.Success(response);
    }
}
