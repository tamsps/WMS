using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.CancelOutbound;

public class CancelOutboundCommand : IRequest<Result<OutboundDto>>
{
    public Guid OutboundId { get; set; }
    public string CurrentUser { get; set; } = null!;
}
