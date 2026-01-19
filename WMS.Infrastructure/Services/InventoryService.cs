using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Inventory;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Inventory> _inventoryRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        WMSDbContext context,
        IRepository<Inventory> inventoryRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InventoryDto>> GetByIdAsync(Guid id)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Location)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory == null)
        {
            return Result<InventoryDto>.Failure("Inventory not found");
        }

        return Result<InventoryDto>.Success(MapToDto(inventory));
    }

    public async Task<Result<PagedResult<InventoryDto>>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Location)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var inventories = await query
            .OrderBy(i => i.Product.SKU)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<InventoryDto>
        {
            Items = inventories.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<InventoryDto>>.Success(result);
    }

    public async Task<Result<InventoryLevelDto>> GetInventoryByProductAsync(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return Result<InventoryLevelDto>.Failure("Product not found");
        }

        var inventories = await _context.Inventories
            .Include(i => i.Location)
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        var dto = new InventoryLevelDto
        {
            ProductId = product.Id,
            ProductSKU = product.SKU,
            ProductName = product.Name,
            TotalQuantity = inventories.Sum(i => i.QuantityOnHand),
            TotalReserved = inventories.Sum(i => i.QuantityReserved),
            TotalAvailable = inventories.Sum(i => i.QuantityAvailable),
            LocationInventories = inventories.Select(i => new LocationInventoryDto
            {
                LocationId = i.LocationId,
                LocationCode = i.Location.Code,
                Quantity = i.QuantityOnHand,
                Reserved = i.QuantityReserved,
                Available = i.QuantityAvailable
            }).ToList()
        };

        return Result<InventoryLevelDto>.Success(dto);
    }

    public async Task<Result<PagedResult<InventoryLevelDto>>> GetInventoryLevelsAsync(
        int pageNumber, int pageSize, string? searchTerm = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.SKU.Contains(searchTerm) || p.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.SKU)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var levels = new List<InventoryLevelDto>();
        foreach (var product in products)
        {
            var inventories = await _context.Inventories
                .Include(i => i.Location)
                .Where(i => i.ProductId == product.Id)
                .ToListAsync();

            levels.Add(new InventoryLevelDto
            {
                ProductId = product.Id,
                ProductSKU = product.SKU,
                ProductName = product.Name,
                TotalQuantity = inventories.Sum(i => i.QuantityOnHand),
                TotalReserved = inventories.Sum(i => i.QuantityReserved),
                TotalAvailable = inventories.Sum(i => i.QuantityAvailable),
                LocationInventories = inventories.Select(i => new LocationInventoryDto
                {
                    LocationId = i.LocationId,
                    LocationCode = i.Location.Code,
                    Quantity = i.QuantityOnHand,
                    Reserved = i.QuantityReserved,
                    Available = i.QuantityAvailable
                }).ToList()
            });
        }

        var result = new PagedResult<InventoryLevelDto>
        {
            Items = levels,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<InventoryLevelDto>>.Success(result);
    }

    public async Task<Result<PagedResult<InventoryTransactionDto>>> GetTransactionsAsync(
        int pageNumber, int pageSize, Guid? productId = null, Guid? locationId = null)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .Include(t => t.Location)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(t => t.LocationId == locationId.Value);
        }

        var totalCount = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<InventoryTransactionDto>
        {
            Items = transactions.Select(MapTransactionToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<InventoryTransactionDto>>.Success(result);
    }

    public async Task<Result<decimal>> GetAvailableQuantityAsync(Guid productId, Guid locationId)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);

        if (inventory == null)
        {
            return Result<decimal>.Success(0);
        }

        return Result<decimal>.Success(inventory.QuantityAvailable);
    }

    // Helper method to update inventory (used by Inbound/Outbound services)
    public async Task<Result<InventoryDto>> UpdateInventoryAsync(
        Guid productId, 
        Guid locationId, 
        decimal quantity, 
        TransactionType transactionType,
        string referenceNumber,
        Guid? referenceId,
        string currentUser)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);

            decimal balanceBefore = inventory?.QuantityOnHand ?? 0;

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = productId,
                    LocationId = locationId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    CreatedBy = currentUser
                };
                await _context.Inventories.AddAsync(inventory);
                await _context.SaveChangesAsync(); // Save to load navigation properties
                
                // Reload with navigation properties
                inventory = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Location)
                    .FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);
            }

            if (transactionType == TransactionType.Inbound)
            {
                inventory.QuantityOnHand += quantity;
            }
            else if (transactionType == TransactionType.Outbound)
            {
                if (inventory.QuantityAvailable < quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<InventoryDto>.Failure("Insufficient available quantity");
                }
                inventory.QuantityOnHand -= quantity;
            }

            inventory.LastStockDate = DateTime.UtcNow;
            inventory.UpdatedBy = currentUser;
            inventory.UpdatedAt = DateTime.UtcNow;
            
            decimal balanceAfter = inventory.QuantityOnHand;

            var transaction = new InventoryTransaction
            {
                TransactionNumber = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 50),
                TransactionType = transactionType,
                ProductId = productId,
                LocationId = locationId,
                Quantity = quantity,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceNumber = referenceNumber,
                ReferenceType = transactionType.ToString(),
                ReferenceId = referenceId,
                CreatedBy = currentUser
            };

            await _context.InventoryTransactions.AddAsync(transaction);
            await _unitOfWork.CommitTransactionAsync();

            return Result<InventoryDto>.Success(MapToDto(inventory!), "Inventory updated successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<InventoryDto>.Failure($"Failed to update inventory: {ex.Message}");
        }
    }

    private static InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductSKU = inventory.Product.SKU,
            ProductName = inventory.Product.Name,
            LocationId = inventory.LocationId,
            LocationCode = inventory.Location.Code,
            LocationName = inventory.Location.Name,
            QuantityOnHand = inventory.QuantityOnHand,
            QuantityReserved = inventory.QuantityReserved,
            QuantityAvailable = inventory.QuantityAvailable,
            LastStockDate = inventory.LastStockDate
        };
    }

    private static InventoryTransactionDto MapTransactionToDto(InventoryTransaction transaction)
    {
        return new InventoryTransactionDto
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            TransactionType = transaction.TransactionType.ToString(),
            ProductId = transaction.ProductId,
            ProductSKU = transaction.Product.SKU,
            ProductName = transaction.Product.Name,
            LocationId = transaction.LocationId,
            LocationCode = transaction.Location.Code,
            Quantity = transaction.Quantity,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            ReferenceType = transaction.ReferenceType,
            ReferenceNumber = transaction.ReferenceNumber,
            CreatedAt = transaction.CreatedAt,
            CreatedBy = transaction.CreatedBy
        };
    }
}
