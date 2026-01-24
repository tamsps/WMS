# ?? CQRS Implementation & Service Migration Plan

## ?? Current State

### Services in WMS.Infrastructure (To Be Moved):
1. ? AuthService.cs ? WMS.Auth.API
2. ? TokenService.cs ? WMS.Auth.API  
3. ? InboundService.cs ? WMS.Inbound.API
4. ? OutboundService.cs ? WMS.Outbound.API
5. ? InventoryService.cs ? WMS.Inventory.API
6. ? LocationService.cs ? WMS.Locations.API
7. ? ProductService.cs ? WMS.Products.API
8. ? PaymentService.cs ? WMS.Payment.API
9. ? DeliveryService.cs ? WMS.Delivery.API

### Target Architecture (CQRS Pattern):

```
WMS.[Service].API/
??? Application/
?   ??? Commands/
?   ?   ??? Create[Entity]/
?   ?   ?   ??? Create[Entity]Command.cs
?   ?   ?   ??? Create[Entity]CommandHandler.cs
?   ?   ?   ??? Create[Entity]CommandValidator.cs
?   ?   ??? Update[Entity]/
?   ?   ??? Delete[Entity]/
?   ??? Queries/
?   ?   ??? Get[Entity]ById/
?   ?   ?   ??? Get[Entity]ByIdQuery.cs
?   ?   ?   ??? Get[Entity]ByIdQueryHandler.cs
?   ?   ??? GetAll[Entity]s/
?   ?       ??? GetAll[Entity]sQuery.cs
?   ?       ??? GetAll[Entity]sQueryHandler.cs
?   ??? Mappers/
?       ??? [Entity]Mapper.cs
??? Services/ (temporary - contains migrated service)
?   ??? [Service]Service.cs
??? DTOs/ (already exists)
??? Interfaces/ (already exists)
??? Common/Models/ (already exists)
??? Controllers/
?   ??? [Entity]Controller.cs (will be updated to use MediatR)
??? Program.cs (will add MediatR)
```

---

## ?? Implementation Steps

### Phase 1: Install Required Packages (All Microservices)

**Packages to Install:**
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

**Microservices:**
1. WMS.Inbound.API
2. WMS.Outbound.API
3. WMS.Inventory.API
4. WMS.Locations.API
5. WMS.Products.API
6. WMS.Payment.API
7. WMS.Delivery.API
8. WMS.Auth.API

---

### Phase 2: Move Services from Infrastructure to Microservices

**For Each Service:**

1. **Create Services folder** in target microservice
2. **Copy service file** from WMS.Infrastructure\Services
3. **Update namespaces** to match microservice
4. **Update using statements** to use microservice's DTOs and Interfaces
5. **Update DbContext reference** to use WMS.Domain.Data.WMSDbContext

**Example for InboundService:**

**FROM:** `WMS.Infrastructure\Services\InboundService.cs`
```csharp
namespace WMS.Infrastructure.Services;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Inbound;
using WMS.Application.Interfaces;
using WMS.Infrastructure.Data;
```

**TO:** `WMS.Inbound.API\Services\InboundService.cs`
```csharp
namespace WMS.Inbound.API.Services;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;
using WMS.Inbound.API.Interfaces;
using WMS.Domain.Data;
```

---

### Phase 3: Implement CQRS for WMS.Inbound.API (Prototype)

#### Commands to Implement:

1. **CreateInboundCommand**
   - Input: CreateInboundDto
   - Output: Result<InboundDto>
   - Handler: Validates products/locations, creates inbound
   - Validator: FluentValidation rules

2. **ReceiveInboundCommand**
   - Input: ReceiveInboundDto
   - Output: Result<InboundDto>
   - Handler: Updates inventory, changes status
   - Validator: Validates received quantities

3. **CancelInboundCommand**
   - Input: Guid id
   - Output: Result
   - Handler: Cancels inbound if status allows

#### Queries to Implement:

1. **GetInboundByIdQuery**
   - Input: Guid id
   - Output: Result<InboundDto>
   - Handler: Fetches with includes

2. **GetAllInboundsQuery**
   - Input: pageNumber, pageSize, status filter
   - Output: Result<PagedResult<InboundDto>>
   - Handler: Paged query with filtering

#### Mapper:

**InboundMapper.cs** - Static mapping methods

---

### Phase 4: Update Controllers to Use MediatR

**Before (using service directly):**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateInboundDto dto)
{
    var result = await _inboundService.CreateAsync(dto, currentUser);
    // ...
}
```

**After (using MediatR):**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateInboundDto dto)
{
    var command = new CreateInboundCommand 
    { 
        Dto = dto, 
        CurrentUser = User.Identity?.Name ?? "System" 
    };
    var result = await _mediator.Send(command);
    // ...
}
```

---

### Phase 5: Update Program.cs

**Add MediatR Registration:**
```csharp
// MediatR - Register all handlers from current assembly
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

**Remove or keep service registration (temporary):**
```csharp
// Can keep temporarily during migration
builder.Services.AddScoped<IInboundService, InboundService>();
```

---

### Phase 6: Replicate for All Microservices

**Apply same pattern to:**
1. WMS.Outbound.API (CreateOutbound, PickOutbound, ShipOutbound, CancelOutbound)
2. WMS.Inventory.API (UpdateInventory, GetInventory, TransferInventory)
3. WMS.Locations.API (CreateLocation, UpdateLocation, DeleteLocation)
4. WMS.Products.API (CreateProduct, UpdateProduct, DeleteProduct)
5. WMS.Payment.API (CreatePayment, ProcessPayment, RefundPayment)
6. WMS.Delivery.API (CreateDelivery, UpdateStatus, CompleteDelivery)
7. WMS.Auth.API (Login, Register, RefreshToken)

---

### Phase 7: Clean Up

1. **Remove services from WMS.Infrastructure\Services**
2. **Update all Program.cs** to remove old service registrations
3. **Verify all controllers** use MediatR
4. **Run tests** to ensure everything works
5. **Update documentation**

---

## ?? Detailed Implementation for WMS.Inbound.API

### Step 1: Install Packages

```powershell
cd WMS.Inbound.API
dotnet add package MediatR --version 12.4.1
dotnet add package FluentValidation --version 11.11.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.11.0
```

### Step 2: Create Folder Structure

```
WMS.Inbound.API/
??? Application/
    ??? Commands/
    ?   ??? CreateInbound/
    ?   ??? ReceiveInbound/
    ?   ??? CancelInbound/
    ??? Queries/
    ?   ??? GetInboundById/
    ?   ??? GetAllInbounds/
    ??? Mappers/
```

### Step 3: Implement CreateInbound Command

**Application/Commands/CreateInbound/CreateInboundCommand.cs:**
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

**Application/Commands/CreateInbound/CreateInboundCommandHandler.cs:**
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
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Status == ProductStatus.Inactive)
            {
                return Result<InboundDto>.Failure($"Product {item.ProductId} is invalid or inactive");
            }

            var location = await _context.Locations.FindAsync(item.LocationId);
            if (location == null || !location.IsActive)
            {
                return Result<InboundDto>.Failure($"Location {item.LocationId} is invalid or inactive");
            }
        }

        var inbound = new Inbound
        {
            InboundNumber = await GenerateInboundNumberAsync(),
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

        // Fetch complete entity
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

    private async Task<string> GenerateInboundNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"IB-{today:yyyyMMdd}";
        
        var lastInbound = await _context.Inbounds
            .Where(i => i.InboundNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InboundNumber)
            .FirstOrDefaultAsync();

        if (lastInbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastInbound.InboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
```

**Application/Commands/CreateInbound/CreateInboundCommandValidator.cs:**
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
            .NotEmpty().WithMessage("Expected date is required");

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

## ?? Benefits After Implementation

### 1. **Clean Architecture Compliance**
- ? Each microservice is self-contained
- ? Business logic in Application layer
- ? Clear separation of concerns

### 2. **CQRS Pattern Benefits**
- ? Read/Write separation
- ? Optimized queries
- ? Better scalability
- ? Easier caching strategies

### 3. **Testability**
- ? Handlers can be unit tested in isolation
- ? Validators are separate and testable
- ? No dependencies on infrastructure

### 4. **Maintainability**
- ? Single Responsibility Principle
- ? Easy to add new commands/queries
- ? Clear naming conventions

---

## ?? Migration Status

| Microservice | Service Moved | CQRS Implemented | Controller Updated | Status |
|--------------|---------------|------------------|-------------------|---------|
| WMS.Inbound.API | ? | ? | ? | Pending |
| WMS.Outbound.API | ? | ? | ? | Pending |
| WMS.Inventory.API | ? | ? | ? | Pending |
| WMS.Locations.API | ? | ? | ? | Pending |
| WMS.Products.API | ? | ? | ? | Pending |
| WMS.Payment.API | ? | ? | ? | Pending |
| WMS.Delivery.API | ? | ? | ? | Pending |
| WMS.Auth.API | ? | ? | ? | Pending |

---

## ?? Ready to Start Implementation

**Should I proceed with:**
1. Installing MediatR packages in all microservices?
2. Implementing complete CQRS for WMS.Inbound.API as prototype?
3. Moving all services to their microservices?

Let me know and I'll execute the implementation!
