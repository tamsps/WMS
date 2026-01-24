using MediatR;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Commands.CreateLocation;

public class CreateLocationCommand : IRequest<Result<LocationDto>>
{
    public CreateLocationDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
