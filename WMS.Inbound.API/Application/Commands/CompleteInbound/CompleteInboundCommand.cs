using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CompleteInbound;

/// <summary>
/// Command to mark an inbound as completed
/// Transitions from PutAway to Completed status
/// </summary>
public class CompleteInboundCommand : IRequest<Result<InboundDto>>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
