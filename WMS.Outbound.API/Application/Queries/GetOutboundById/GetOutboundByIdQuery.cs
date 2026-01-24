using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Queries.GetOutboundById;

public class GetOutboundByIdQuery : IRequest<Result<OutboundDto>>
{
    public Guid Id { get; set; }
}
