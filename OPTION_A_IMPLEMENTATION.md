# ?? Option A Implementation - Complete Refactoring

## Overview
Implementing Clean Architecture for WMS microservices with full CQRS pattern.

---

## Phase 1: Preparation (CURRENT)

### ? Step 1.1: Move Repositories to WMS.Domain
- [x] Create WMS.Domain\Repositories\Repository.cs
- [x] Create WMS.Domain\Repositories\UnitOfWork.cs
- [x] Update namespace from `WMS.Infrastructure.Repositories` to `WMS.Domain.Repositories`
- [x] Update DbContext reference from `WMS.Infrastructure.Data` to `WMS.Domain.Data`

### ? Step 1.2: Remove WMS.Application Project
**Why:** DTOs and Interfaces are already duplicated in each microservice

**Verification:**
- DTOs already exist in each microservice (e.g., WMS.Inbound.API\DTOs\Inbound\InboundDto.cs)
- Interfaces already exist in each microservice (e.g., WMS.Inbound.API\Interfaces\IInboundService.cs)
- Common models (Result, PagedResult) already in each microservice

**Action Required:**
1. Remove WMS.Application project reference from all projects
2. Delete WMS.Application folder
3. Update solution file

### ? Step 1.3: Move Services from WMS.Infrastructure to Microservices

**Current Dependencies (Need to Fix):**
```csharp
// Services currently use:
using WMS.Application.Common.Models;        // ? Move to microservice's Common.Models
using WMS.Application.DTOs.Inbound;         // ? Move to microservice's DTOs
using WMS.Application.Interfaces;           // ? Move to microservice's Interfaces
using WMS.Infrastructure.Data;              // ? Change to WMS.Domain.Data
```

**Services to Move:**

| Service | Lines of Code | Target Microservice | Complexity |
|---------|---------------|---------------------|------------|
| InboundService.cs | ~300 | WMS.Inbound.API\Services | Medium |
| OutboundService.cs | ~350 | WMS.Outbound.API\Services | Medium-High |
| InventoryService.cs | ~400 | WMS.Inventory.API\Services | High |
| LocationService.cs | ~200 | WMS.Locations.API\Services | Low |
| ProductService.cs | ~250 | WMS.Products.API\Services | Low-Medium |
| PaymentService.cs | ~300 | WMS.Payment.API\Services | Medium |
| DeliveryService.cs | ~350 | WMS.Delivery.API\Services | Medium |
| AuthService.cs | ~200 | WMS.Auth.API\Services | Low-Medium |
| TokenService.cs | ~100 | WMS.Auth.API\Services | Low |

---

## Phase 2: Implement CQRS for WMS.Inbound.API (PROTOTYPE)

### Step 2.1: Install Required Packages

**Add to WMS.Inbound.API.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.4.1" />
  <PackageReference Include="FluentValidation" Version="11.11.0" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
</ItemGroup>
```

### Step 2.2: Create Application Layer Structure

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
?   ??? InboundService.cs (moved from Infrastructure)
```

### Step 2.3: Update Program.cs

```csharp
using WMS.Domain.Data;                    // ? Changed from WMS.Infrastructure.Data
using WMS.Domain.Repositories;            // ? Changed from WMS.Infrastructure.Repositories
using WMS.Inbound.API.Services;           // ? Changed from WMS.Infrastructure.Services
using MediatR;                             // ? NEW

// Database
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("WMS.Domain")));  // ? Changed from WMS.Infrastructure

// MediatR - NEW
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Validation - NEW
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services (temporary - will be replaced by CQRS)
builder.Services.AddScoped<IInboundService, InboundService>();
```

### Step 2.4: Update Controller to Use MediatR

**Before:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateInboundDto dto)
{
    var result = await _inboundService.CreateAsync(dto, currentUser);
    // ...
}
```

**After:**
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

## Phase 3: Replicate for Other Microservices

After WMS.Inbound.API prototype is complete and tested:

1. **WMS.Outbound.API** - Copy CQRS pattern
2. **WMS.Inventory.API** - Copy CQRS pattern
3. **WMS.Locations.API** - Copy CQRS pattern
4. **WMS.Products.API** - Copy CQRS pattern
5. **WMS.Payment.API** - Copy CQRS pattern
6. **WMS.Delivery.API** - Copy CQRS pattern
7. **WMS.Auth.API** - Copy CQRS pattern

---

## Phase 4: Cleanup

### Remove Old Infrastructure
- [ ] Delete WMS.Infrastructure\Services folder
- [ ] Delete WMS.Infrastructure\Repositories folder
- [ ] Update WMS.Infrastructure.csproj (remove obsolete references)
- [ ] Or remove WMS.Infrastructure entirely if empty

### Update All Program.cs Files
- [ ] Change all `using WMS.Infrastructure.Data` to `using WMS.Domain.Data`
- [ ] Change all `using WMS.Infrastructure.Repositories` to `using WMS.Domain.Repositories`
- [ ] Change all migration assembly from `"WMS.Infrastructure"` to `"WMS.Domain"`

### Verify Build
- [ ] Build entire solution
- [ ] Run tests (if any)
- [ ] Verify all microservices start successfully

---

## File Count Estimate

### Files to Create (per microservice):
- 3-5 Commands (with handlers and validators) = ~15 files
- 2-3 Queries (with handlers) = ~6 files
- 1 Mapper = ~1 file
- **Total per microservice: ~22 files**

### Total for All 8 Microservices:
- **~176 new files**

### Files to Delete:
- WMS.Application: ~20 files
- WMS.Infrastructure\Services: ~9 files
- WMS.Infrastructure\Repositories: ~2 files (moved to Domain)
- **Total: ~31 files deleted**

---

## Time Estimate

### Automated (Option A - Current Choice):
- **Phase 1 (Preparation):** 30 minutes
- **Phase 2 (Inbound Prototype):** 1-2 hours
- **Phase 3 (Replicate 7x):** 3-4 hours
- **Phase 4 (Cleanup & Verification):** 1 hour

**Total:** 5-7 hours of implementation

### Manual (If you were doing it yourself):
- **10-15 hours**

---

## Benefits After Completion

? **Clean Architecture Compliance**
- Clear separation of concerns
- Each microservice is self-contained
- CQRS pattern for better scalability

? **Improved Maintainability**
- Easier to understand business logic
- Command/Query handlers are focused and testable
- Validators ensure data integrity

? **Better Testability**
- Handlers can be unit tested in isolation
- No dependencies on external services
- Clear inputs and outputs

? **Independent Deployment**
- Each microservice can evolve separately
- No shared WMS.Application dependency
- Domain project provides necessary stability

? **Scalability**
- Can scale read (queries) and write (commands) independently
- Easier to add caching strategies
- Better performance optimization opportunities

---

## Current Status

**Phase:** 1 - Preparation  
**Step:** 1.1 Complete (Repositories moved to Domain)  
**Next:** Move services from Infrastructure to microservices

**Ready to proceed with automated implementation?**

Let me know and I'll execute the complete refactoring!
