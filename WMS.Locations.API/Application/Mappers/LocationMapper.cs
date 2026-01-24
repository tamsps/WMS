using WMS.Domain.Entities;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Mappers;

public static class LocationMapper
{
    public static LocationDto MapToDto(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Code = location.Code,
            Name = location.Name,
            Description = location.Description,
            Zone = location.Zone,
            Aisle = location.Aisle,
            Rack = location.Rack,
            Shelf = location.Shelf,
            Bin = location.Bin,
            Capacity = location.Capacity,
            CurrentOccupancy = location.CurrentOccupancy,
            IsActive = location.IsActive,
            LocationType = location.LocationType,
            ParentLocationId = location.ParentLocationId
        };
    }
}
