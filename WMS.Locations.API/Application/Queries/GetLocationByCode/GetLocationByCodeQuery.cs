using MediatR;
using WMS.Locations.API.Common.Models;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Application.Queries.GetLocationByCode;

public class GetLocationByCodeQuery : IRequest<Result<LocationDto>>
{
    public string Code { get; set; } = null!;
}
