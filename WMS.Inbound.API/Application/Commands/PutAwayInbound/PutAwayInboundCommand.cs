using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.PutAwayInbound;

/// <summary>
/// Command to mark an inbound as put away
/// Transitions from Received to PutAway status
/// </summary>
public class PutAwayInboundCommand : IRequest<Result<InboundDto>>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
