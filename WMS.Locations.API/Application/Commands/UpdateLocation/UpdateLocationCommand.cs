using MediatR;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Commands.UpdateLocation;

public class UpdateLocationCommand : IRequest<Result<LocationDto>>
{
    public UpdateLocationDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
