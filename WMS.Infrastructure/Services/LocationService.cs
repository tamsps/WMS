using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Location;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Location> _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationService(
        WMSDbContext context,
        IRepository<Location> locationRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LocationDto>> GetByIdAsync(Guid id)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            return Result<LocationDto>.Failure("Location not found");
        }

        return Result<LocationDto>.Success(MapToDto(location));
    }

    public async Task<Result<LocationDto>> GetByCodeAsync(string code)
    {
        var location = await _locationRepository.FirstOrDefaultAsync(l => l.Code == code);
        if (location == null)
        {
            return Result<LocationDto>.Failure("Location not found");
        }

        return Result<LocationDto>.Success(MapToDto(location));
    }

    public async Task<Result<PagedResult<LocationDto>>> GetAllAsync(int pageNumber, int pageSize, string? zone = null)
    {
        var query = _context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(zone))
        {
            query = query.Where(l => l.Zone == zone);
        }

        var totalCount = await query.CountAsync();
        var locations = await query
            .OrderBy(l => l.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<LocationDto>
        {
            Items = locations.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<LocationDto>>.Success(result);
    }

    public async Task<Result<LocationDto>> CreateAsync(CreateLocationDto dto, string currentUser)
    {
        // Validate code uniqueness
        var exists = await _locationRepository.ExistsAsync(l => l.Code == dto.Code);
        if (exists)
        {
            return Result<LocationDto>.Failure($"Location with code '{dto.Code}' already exists");
        }

        // Validate parent location if specified
        if (dto.ParentLocationId.HasValue)
        {
            var parentExists = await _locationRepository.ExistsAsync(l => l.Id == dto.ParentLocationId.Value);
            if (!parentExists)
            {
                return Result<LocationDto>.Failure("Parent location not found");
            }
        }

        var location = new Location
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Zone = dto.Zone,
            Aisle = dto.Aisle,
            Rack = dto.Rack,
            Shelf = dto.Shelf,
            Bin = dto.Bin,
            Capacity = dto.Capacity,
            CurrentOccupancy = 0,
            IsActive = true,
            LocationType = dto.LocationType,
            ParentLocationId = dto.ParentLocationId,
            CreatedBy = currentUser
        };

        await _locationRepository.AddAsync(location);
        await _unitOfWork.SaveChangesAsync();

        return Result<LocationDto>.Success(MapToDto(location), "Location created successfully");
    }

    public async Task<Result<LocationDto>> UpdateAsync(UpdateLocationDto dto, string currentUser)
    {
        var location = await _locationRepository.GetByIdAsync(dto.Id);
        if (location == null)
        {
            return Result<LocationDto>.Failure("Location not found");
        }

        location.Name = dto.Name;
        location.Description = dto.Description;
        location.Capacity = dto.Capacity;
        location.IsActive = dto.IsActive;
        location.UpdatedBy = currentUser;
        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateAsync(location);
        await _unitOfWork.SaveChangesAsync();

        return Result<LocationDto>.Success(MapToDto(location), "Location updated successfully");
    }

    public async Task<Result> DeactivateAsync(Guid id, string currentUser)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null)
        {
            return Result.Failure("Location not found");
        }

        // Check if location has inventory
        var hasInventory = await _context.Inventories
            .AnyAsync(i => i.LocationId == id && i.QuantityOnHand > 0);

        if (hasInventory)
        {
            return Result.Failure("Cannot deactivate location with existing inventory");
        }

        location.IsActive = false;
        location.UpdatedBy = currentUser;
        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateAsync(location);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Location deactivated successfully");
    }

    private static LocationDto MapToDto(Location location)
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
