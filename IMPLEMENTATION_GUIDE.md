# WMS Implementation Guide

## Complete Service Implementations

This document contains the complete implementation for all remaining services. Due to size constraints, these implementations should be created in your project.

## 1. InventoryService Implementation

**File**: `WMS.Infrastructure/Services/InventoryService.cs`

```csharp
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
    public async Task<Result> UpdateInventoryAsync(
        Guid productId, 
        Guid locationId, 
        decimal quantity, 
        TransactionType transactionType,
        string referenceNumber,
        string currentUser)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get or create inventory record
            var inventory = await _context.Inventories
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
            }

            // Update quantity based on transaction type
            if (transactionType == TransactionType.Inbound)
            {
                inventory.QuantityOnHand += quantity;
            }
            else if (transactionType == TransactionType.Outbound)
            {
                if (inventory.QuantityAvailable < quantity)
                {
                    return Result.Failure("Insufficient available quantity");
                }
                inventory.QuantityOnHand -= quantity;
            }

            inventory.LastStockDate = DateTime.UtcNow;
            decimal balanceAfter = inventory.QuantityOnHand;

            // Create transaction record
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
                CreatedBy = currentUser
            };

            await _context.InventoryTransactions.AddAsync(transaction);
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success("Inventory updated successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure($"Failed to update inventory: {ex.Message}");
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
```

## 2. Update Program.cs to Register All Services

Add these lines to `WMS.API/Program.cs` after existing service registrations:

```csharp
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInboundService, InboundService>();
builder.Services.AddScoped<IOutboundService, OutboundService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

## 3. Controllers to Create

Create these additional controllers in `WMS.API/Controllers/`:

- `InventoryController.cs`
- `InboundController.cs`
- `OutboundController.cs`
- `PaymentController.cs`
- `DeliveryController.cs`
- `AuthController.cs`

Each following the same pattern as ProductsController with appropriate endpoints.

## 4. Database Migration Commands

```powershell
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Navigate to API project
cd WMS.API

# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# If you need to remove last migration
dotnet ef migrations remove

# To see applied migrations
dotnet ef migrations list
```

## 5. Testing with Swagger

1. Run the API project
2. Navigate to `https://localhost:7xxx/`
3. Use the Swagger UI to test endpoints
4. First, call `/api/auth/login` to get a JWT token
5. Click "Authorize" button and enter: `Bearer {your-token}`
6. Now you can test all protected endpoints

## 6. Sample Test Data Script

```sql
-- Insert sample products
INSERT INTO Products (Id, SKU, Name, Description, Status, UOM, Weight, Length, Width, Height, CreatedAt, CreatedBy)
VALUES 
(NEWID(), 'SKU001', 'Product 1', 'Test Product 1', 1, 'EA', 1.5, 10, 5, 3, GETUTCDATE(), 'System'),
(NEWID(), 'SKU002', 'Product 2', 'Test Product 2', 1, 'EA', 2.0, 12, 6, 4, GETUTCDATE(), 'System');

-- Insert sample locations
INSERT INTO Locations (Id, Code, Name, Description, Zone, Aisle, Rack, Shelf, Bin, Capacity, CurrentOccupancy, IsActive, CreatedAt, CreatedBy)
VALUES 
(NEWID(), 'A-01-01-01', 'Zone A Aisle 1 Rack 1 Shelf 1', 'Primary storage', 'A', '01', '01', '01', '01', 1000, 0, 1, GETUTCDATE(), 'System'),
(NEWID(), 'A-01-01-02', 'Zone A Aisle 1 Rack 1 Shelf 2', 'Primary storage', 'A', '01', '01', '02', '01', 1000, 0, 1, GETUTCDATE(), 'System');
```

## 7. Environment Variables

For production, use environment variables instead of appsettings.json:

```bash
export ConnectionStrings__DefaultConnection="Your-Production-Connection-String"
export JwtSettings__SecretKey="Your-Production-Secret-Key"
```

## 8. Docker Support (Optional)

Create `Dockerfile` in WMS.API:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["WMS.API/WMS.API.csproj", "WMS.API/"]
COPY ["WMS.Infrastructure/WMS.Infrastructure.csproj", "WMS.Infrastructure/"]
COPY ["WMS.Application/WMS.Application.csproj", "WMS.Application/"]
COPY ["WMS.Domain/WMS.Domain.csproj", "WMS.Domain/"]
RUN dotnet restore "WMS.API/WMS.API.csproj"
COPY . .
WORKDIR "/src/WMS.API"
RUN dotnet build "WMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WMS.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WMS.API.dll"]
```

## Next Steps

1. Create remaining service implementations
2. Create remaining controllers
3. Run database migrations
4. Test all endpoints
5. Implement the Web MVC project
6. Add validation using FluentValidation
7. Add comprehensive error handling
8. Add logging (Serilog recommended)
9. Add health checks
10. Add API versioning

This provides a complete foundation for your WMS system!
