using MediatR;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetLocationById;

public class GetLocationByIdQuery : IRequest<Result<LocationDto>>
{
    public Guid Id { get; set; }
}
