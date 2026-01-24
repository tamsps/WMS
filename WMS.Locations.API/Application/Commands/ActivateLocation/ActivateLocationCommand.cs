using MediatR;
using WMS.Locations.API.Common.Models;

namespace WMS.Locations.API.Application.Commands.ActivateLocation;

public class ActivateLocationCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
