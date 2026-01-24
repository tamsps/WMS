using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Interfaces;

public interface ILocationService
{
    Task<Result<LocationDto>> GetByIdAsync(Guid id);
    Task<Result<LocationDto>> GetByCodeAsync(string code);
    Task<Result<PagedResult<LocationDto>>> GetAllAsync(int pageNumber, int pageSize, string? zone = null);
    Task<Result<LocationDto>> CreateAsync(CreateLocationDto dto, string currentUser);
    Task<Result<LocationDto>> UpdateAsync(UpdateLocationDto dto, string currentUser);
    Task<Result> DeactivateAsync(Guid id, string currentUser);
}
