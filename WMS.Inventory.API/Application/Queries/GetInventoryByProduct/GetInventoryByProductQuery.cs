using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryByProduct;

public class GetInventoryByProductQuery : IRequest<Result<InventoryLevelDto>>
{
    public Guid ProductId { get; set; }
}
