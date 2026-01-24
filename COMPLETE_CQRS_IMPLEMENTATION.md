# ?? Complete CQRS Implementation Guide - WMS Microservices

## ? Step 1: Add Packages to All Microservices (COMPLETED for Inbound)

Run this for each microservice or update `.csproj` files:

```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

**Status:**
- [x] WMS.Inbound.API
- [ ] WMS.Outbound.API
- [ ] WMS.Inventory.API
- [ ] WMS.Locations.API
- [ ] WMS.Products.API
- [ ] WMS.Payment.API
- [ ] WMS.Delivery.API
- [ ] WMS.Auth.API

---

## ?? Step 2: Complete File Structure for WMS.Inbound.API

I'll provide ALL files needed for the complete CQRS implementation.

### Directory Structure:
```
WMS.Inbound.API/
??? Application/
?   ??? Commands/
?   ?   ??? CreateInbound/
?   ?   ?   ??? CreateInboundCommand.cs
?   ?   ?   ??? CreateInboundCommandHandler.cs
?   ?   ?   ??? CreateInboundCommandValidator.cs
?   ?   ??? ReceiveInbound/
?   ?   ?   ??? ReceiveInboundCommand.cs
?   ?   ?   ??? ReceiveInboundCommandHandler.cs
?   ?   ?   ??? ReceiveInboundCommandValidator.cs
?   ?   ??? CancelInbound/
?   ?       ??? CancelInboundCommand.cs
?   ?       ??? CancelInboundCommandHandler.cs
?   ??? Queries/
?   ?   ??? GetInboundById/
?   ?   ?   ??? GetInboundByIdQuery.cs
?   ?   ?   ??? GetInboundByIdQueryHandler.cs
?   ?   ??? GetAllInbounds/
?   ?       ??? GetAllInboundsQuery.cs
?   ?       ??? GetAllInboundsQueryHandler.cs
?   ??? Mappers/
?       ??? InboundMapper.cs
??? Services/
?   ??? InboundService.cs (migrated from Infrastructure)
??? Controllers/
    ??? InboundController.cs (updated to use MediatR)
```

---

## ?? Complete File Implementation

I'll create all necessary files. This is a production-ready implementation following best practices.

---

### ?? Application/Mappers/InboundMapper.cs

```csharp
using WMS.Domain.Entities;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Mappers;

public static class InboundMapper
{
    public static InboundDto MapToDto(Inbound inbound)
    {
        return new InboundDto
        {
            Id = inbound.Id,
            InboundNumber = inbound.InboundNumber,
            ReferenceNumber = inbound.ReferenceNumber,
            Status = inbound.Status.ToString(),
            ExpectedDate = inbound.ExpectedDate,
            ReceivedDate = inbound.ReceivedDate,
            SupplierName = inbound.SupplierName,
            SupplierCode = inbound.SupplierCode,
            Notes = inbound.Notes,
            Items = inbound.InboundItems.Select(ii => new InboundItemDto
            {
                Id = ii.Id,
                ProductId = ii.ProductId,
                ProductSKU = ii.Product.SKU,
                ProductName = ii.Product.Name,
                LocationId = ii.LocationId,
                LocationCode = ii.Location.Code,
                LocationName = ii.Location.Name,
                ExpectedQuantity = ii.ExpectedQuantity,
                ReceivedQuantity = ii.ReceivedQuantity,
                DamagedQuantity = ii.DamagedQuantity,
                LotNumber = ii.LotNumber,
                ExpiryDate = ii.ExpiryDate,
                Notes = ii.Notes
            }).ToList(),
            CreatedAt = inbound.CreatedAt
        };
    }
}
```

---

### ?? Application/Commands/CreateInbound/CreateInboundCommand.cs

```csharp
using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommand : IRequest<Result<InboundDto>>
{
    public CreateInboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
```

---

### ?? Application/Commands/CreateInbound/CreateInboundCommandHandler.cs

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommandHandler : IRequestHandler<CreateInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Inbound> _inboundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInboundCommandHandler(
        WMSDbContext context,
        IRepository<Inbound> inboundRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _inboundRepository = inboundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(CreateInboundCommand request, CancellationToken cancellationToken)
    {
        // Validate products and locations
        foreach (var item in request.Dto.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            if (product == null || product.Status == ProductStatus.Inactive)
            {
                return Result<InboundDto>.Failure($"Product {item.ProductId} is invalid or inactive");
            }

            var location = await _context.Locations.FindAsync(new object[] { item.LocationId }, cancellationToken);
            if (location == null || !location.IsActive)
            {
                return Result<InboundDto>.Failure($"Location {item.LocationId} is invalid or inactive");
            }
        }

        var inbound = new Inbound
        {
            InboundNumber = await GenerateInboundNumberAsync(cancellationToken),
            ReferenceNumber = request.Dto.ReferenceNumber,
            Status = InboundStatus.Pending,
            ExpectedDate = request.Dto.ExpectedDate,
            SupplierName = request.Dto.SupplierName,
            SupplierCode = request.Dto.SupplierCode,
            Notes = request.Dto.Notes,
            CreatedBy = request.CurrentUser
        };

        foreach (var itemDto in request.Dto.Items)
        {
            inbound.InboundItems.Add(new InboundItem
            {
                ProductId = itemDto.ProductId,
                LocationId = itemDto.LocationId,
                ExpectedQuantity = itemDto.ExpectedQuantity,
                ReceivedQuantity = 0,
                LotNumber = itemDto.LotNumber,
                ExpiryDate = itemDto.ExpiryDate,
                Notes = itemDto.Notes,
                CreatedBy = request.CurrentUser
            });
        }

        await _inboundRepository.AddAsync(inbound, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch complete entity with includes
        var created = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == inbound.Id, cancellationToken);

        return Result<InboundDto>.Success(
            InboundMapper.MapToDto(created!), 
            "Inbound created successfully");
    }

    private async Task<string> GenerateInboundNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"IB-{today:yyyyMMdd}";
        
        var lastInbound = await _context.Inbounds
            .Where(i => i.InboundNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InboundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastInbound.InboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
```

---

### ?? Application/Commands/CreateInbound/CreateInboundCommandValidator.cs

```csharp
using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommandValidator : AbstractValidator<CreateInboundCommand>
{
    public CreateInboundCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Inbound data is required");

        RuleFor(x => x.Dto.SupplierName)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.Dto.ExpectedDate)
            .NotEmpty().WithMessage("Expected date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Expected date cannot be in the past");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product is required");

            item.RuleFor(i => i.LocationId)
                .NotEmpty().WithMessage("Location is required");

            item.RuleFor(i => i.ExpectedQuantity)
                .GreaterThan(0).WithMessage("Expected quantity must be greater than 0");
        });

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
```

---

### ?? Application/Commands/ReceiveInbound/ReceiveInboundCommand.cs

```csharp
using MediatR;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.ReceiveInbound;

public class ReceiveInboundCommand : IRequest<Result<InboundDto>>
{
    public ReceiveInboundDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
```

---

### ?? Application/Commands/ReceiveInbound/ReceiveInboundCommandHandler.cs

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.ReceiveInbound;

public class ReceiveInboundCommandHandler : IRequestHandler<ReceiveInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WMS.Application.Interfaces.IInventoryService _inventoryService;

    public ReceiveInboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork,
        WMS.Application.Interfaces.IInventoryService inventoryService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
    }

    public async Task<Result<InboundDto>> Handle(ReceiveInboundCommand request, CancellationToken cancellationToken)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == request.Dto.InboundId, cancellationToken);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Pending)
        {
            return Result<InboundDto>.Failure($"Cannot receive inbound in {inbound.Status} status");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var receiveItem in request.Dto.Items)
            {
                var inboundItem = inbound.InboundItems.FirstOrDefault(ii => ii.Id == receiveItem.InboundItemId);
                if (inboundItem == null)
                {
                    return Result<InboundDto>.Failure($"Inbound item {receiveItem.InboundItemId} not found");
                }

                inboundItem.ReceivedQuantity = receiveItem.ReceivedQuantity;
                inboundItem.DamagedQuantity = receiveItem.DamagedQuantity;
                if (!string.IsNullOrWhiteSpace(receiveItem.Notes))
                {
                    inboundItem.Notes = receiveItem.Notes;
                }
                inboundItem.UpdatedBy = request.CurrentUser;
                inboundItem.UpdatedAt = DateTime.UtcNow;

                // Update inventory for received quantity (excluding damaged)
                var goodQuantity = receiveItem.ReceivedQuantity - (receiveItem.DamagedQuantity ?? 0);
                if (goodQuantity > 0)
                {
                    var inventoryResult = await _inventoryService.UpdateInventoryAsync(
                        inboundItem.ProductId,
                        inboundItem.LocationId,
                        goodQuantity,
                        TransactionType.Inbound,
                        inbound.InboundNumber,
                        inbound.Id,
                        request.CurrentUser
                    );

                    if (!inventoryResult.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result<InboundDto>.Failure(inventoryResult.Errors.FirstOrDefault() ?? "Failed to update inventory");
                    }
                }
            }

            inbound.Status = InboundStatus.Received;
            inbound.ReceivedDate = DateTime.UtcNow;
            inbound.UpdatedBy = request.CurrentUser;
            inbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<InboundDto>.Success(InboundMapper.MapToDto(inbound), "Inbound received successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InboundDto>.Failure($"Failed to receive inbound: {ex.Message}");
        }
    }
}
```

---

Due to length limitations, I'll create this as a comprehensive guide document. The implementation follows the exact same pattern for all other commands, queries, and microservices. Would you like me to:

1. Continue creating all the remaining CQRS files for WMS.Inbound.API?
2. Create a script to generate the folder structure?
3. Provide the updated Controller and Program.cs files?

Let me know and I'll proceed with the complete implementation!
