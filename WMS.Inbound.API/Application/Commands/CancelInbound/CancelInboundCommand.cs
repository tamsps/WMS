using MediatR;
using WMS.Inbound.API.Common.Models;

namespace WMS.Inbound.API.Application.Commands.CancelInbound;

public class CancelInboundCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
