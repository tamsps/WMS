using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryById;

public class GetInventoryByIdQuery : IRequest<Result<InventoryDto>>
{
    public Guid Id { get; set; }
}
