using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    private readonly IRepository<Location> _locationRepository;

    public GetLocationByIdQueryHandler(IRepository<Location> locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (location == null)
        {
            return Result<LocationDto>.Failure("Location not found");
        }

        return Result<LocationDto>.Success(LocationMapper.MapToDto(location));
    }
}
