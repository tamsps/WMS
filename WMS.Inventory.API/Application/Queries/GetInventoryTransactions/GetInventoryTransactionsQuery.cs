using MediatR;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Application.Queries.GetInventoryTransactions;

public class GetInventoryTransactionsQuery : IRequest<Result<PagedResult<InventoryTransactionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
}
