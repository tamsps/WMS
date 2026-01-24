using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.ShipOutbound;

public class ShipOutboundCommand : IRequest<Result<OutboundDto>>
{
    public ShipOutboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
