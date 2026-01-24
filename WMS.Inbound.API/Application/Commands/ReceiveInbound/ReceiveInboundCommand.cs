using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.ReceiveInbound;

public class ReceiveInboundCommand : IRequest<Result<InboundDto>>
{
    public ReceiveInboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
