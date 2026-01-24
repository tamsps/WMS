using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.CreateOutbound;

public class CreateOutboundCommand : IRequest<Result<OutboundDto>>
{
    public CreateOutboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
