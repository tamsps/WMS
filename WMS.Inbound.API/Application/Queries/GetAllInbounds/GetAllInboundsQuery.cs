using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Queries.GetAllInbounds;

public class GetAllInboundsQuery : IRequest<Result<PagedResult<InboundDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
}
