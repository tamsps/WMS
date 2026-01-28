using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Commands.CreateInventory;

public class CreateInventoryCommand : IRequest<Result<InventoryDto>>
{
    public CreateInventoryDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}

public class CreateInventoryDto
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; } = 0;
    public string? Notes { get; set; }
}