using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Locations.API.Application.Mappers;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Commands.CreateLocation;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Result<LocationDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Location> _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLocationCommandHandler(
        WMSDbContext context,
        IRepository<Location> locationRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LocationDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // Check if Code already exists
        var existingLocation = await _context.Locations
            .FirstOrDefaultAsync(l => l.Code == request.Dto.Code, cancellationToken);

        if (existingLocation != null)
        {
            return Result<LocationDto>.Failure($"Location with code '{request.Dto.Code}' already exists");
        }

        // Validate parent location if specified
        if (request.Dto.ParentLocationId.HasValue)
        {
            var parentLocation = await _context.Locations
                .FindAsync(new object[] { request.Dto.ParentLocationId.Value }, cancellationToken);

            if (parentLocation == null)
            {
                return Result<LocationDto>.Failure("Parent location not found");
            }
        }

        var location = new Location
        {
            Code = request.Dto.Code,
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Zone = request.Dto.Zone,
            Aisle = request.Dto.Aisle,
            Rack = request.Dto.Rack,
            Shelf = request.Dto.Shelf,
            Bin = request.Dto.Bin,
            Capacity = request.Dto.Capacity,
            CurrentOccupancy = 0,
            IsActive = true,
            LocationType = request.Dto.LocationType,
            ParentLocationId = request.Dto.ParentLocationId,
            CreatedBy = request.CurrentUser
        };

        await _locationRepository.AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LocationDto>.Success(
            LocationMapper.MapToDto(location),
            "Location created successfully");
    }
}
