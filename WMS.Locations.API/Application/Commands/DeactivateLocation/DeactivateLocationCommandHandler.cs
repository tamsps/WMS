using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Locations.API.Common.Models;

namespace WMS.Locations.API.Application.Commands.DeactivateLocation;

public class DeactivateLocationCommandHandler : IRequestHandler<DeactivateLocationCommand, Result>
{
    private readonly IRepository<Location> _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateLocationCommandHandler(
        IRepository<Location> locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (location == null)
        {
            return Result.Failure("Location not found");
        }

        location.IsActive = false;
        location.UpdatedBy = request.CurrentUser;
        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateAsync(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Location deactivated successfully");
    }
}
