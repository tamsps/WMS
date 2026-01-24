using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetAllInventory;

public class GetAllInventoryQuery : IRequest<Result<PagedResult<InventoryDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
