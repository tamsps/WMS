using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryByLocation;

public class GetInventoryByLocationQuery : IRequest<Result<PagedResult<InventoryDto>>>
{
    public Guid LocationId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
