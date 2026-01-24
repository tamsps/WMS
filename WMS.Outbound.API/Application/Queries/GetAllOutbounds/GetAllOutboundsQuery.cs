using MediatR;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Queries.GetAllOutbounds;

public class GetAllOutboundsQuery : IRequest<Result<PagedResult<OutboundDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
}
