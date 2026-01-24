using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.PickOutbound;

public class PickOutboundCommand : IRequest<Result<OutboundDto>>
{
    public PickOutboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
