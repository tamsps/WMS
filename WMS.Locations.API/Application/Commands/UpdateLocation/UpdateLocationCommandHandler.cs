using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Commands.UpdateLocation;

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result<LocationDto>>
{
    private readonly IRepository<Location> _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLocationCommandHandler(
        IRepository<Location> locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LocationDto>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.Dto.Id, cancellationToken);
        if (location == null)
        {
            return Result<LocationDto>.Failure("Location not found");
        }

        // Validate capacity cannot be reduced below current occupancy
        if (request.Dto.Capacity < location.CurrentOccupancy)
        {
            return Result<LocationDto>.Failure(
                $"Cannot reduce capacity to {request.Dto.Capacity} because current occupancy is {location.CurrentOccupancy}. " +
                $"Please remove inventory first or increase the capacity value.");
        }

        location.Name = request.Dto.Name;
        location.Description = request.Dto.Description;
        location.Capacity = request.Dto.Capacity;
        location.IsActive = request.Dto.IsActive;
        location.UpdatedBy = request.CurrentUser;
        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateAsync(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LocationDto>.Success(
            LocationMapper.MapToDto(location),
            "Location updated successfully");
    }
}
