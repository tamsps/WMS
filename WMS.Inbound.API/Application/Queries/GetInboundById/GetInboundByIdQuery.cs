using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Queries.GetInboundById;

public class GetInboundByIdQuery : IRequest<Result<InboundDto>>
{
    public Guid Id { get; set; }
}
