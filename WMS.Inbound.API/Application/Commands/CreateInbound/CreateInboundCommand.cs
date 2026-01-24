using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommand : IRequest<Result<InboundDto>>
{
    public CreateInboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
