using WMS.Application.Common.Models;
using WMS.Application.DTOs.Inventory;
using WMS.Domain.Enums;

namespace WMS.Application.Interfaces;

public interface IInventoryService
{
    Task<Result<InventoryDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<InventoryDto>>> GetAllAsync(int pageNumber, int pageSize);
    Task<Result<InventoryLevelDto>> GetInventoryByProductAsync(Guid productId);
    Task<Result<PagedResult<InventoryLevelDto>>> GetInventoryLevelsAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<Result<PagedResult<InventoryTransactionDto>>> GetTransactionsAsync(int pageNumber, int pageSize, Guid? productId = null, Guid? locationId = null);
    Task<Result<decimal>> GetAvailableQuantityAsync(Guid productId, Guid locationId);
    Task<Result<InventoryDto>> UpdateInventoryAsync(Guid productId, Guid locationId, decimal quantity, TransactionType transactionType, string referenceNumber, Guid? referenceId, string currentUser);
}
