# ? Clean Architecture Refactoring - Quick Reference

## ?? What We're Doing
Refactoring WMS microservices to follow Clean Architecture with:
- Each microservice has its own Application layer (CQRS pattern)
- Shared Domain project (entities + DbContext + migrations)
- Independent deployability per microservice

## ? Completed
- [x] DbContext moved to WMS.Domain
- [x] EF Core packages added to WMS.Domain
- [x] Build verified successful
- [x] Documentation created

## ?? Key Files to Read

| Priority | File | What It Contains |
|----------|------|------------------|
| **START HERE** | **REFACTORING_SUMMARY.md** | Overview, decisions, next steps |
| **STEP-BY-STEP** | **CLEAN_ARCHITECTURE_IMPLEMENTATION.md** | Detailed implementation guide |
| **ARCHITECTURE** | **CLEAN_ARCHITECTURE_REFACTORING.md** | Full architecture explanation |

## ?? Next Actions (Choose One)

### Option A: I Implement Prototype ? RECOMMENDED
**What I'll do:**
1. Install MediatR and FluentValidation in all microservices
2. Update all Program.cs to use WMS.Domain.Data.WMSDbContext
3. Implement complete CQRS for WMS.Inbound.API as prototype
4. Provide replication guide for other microservices

**Time:** 2-3 hours (automated)
**You get:** Working prototype to replicate

### Option B: You Implement Manually
**What you'll do:**
1. Follow CLEAN_ARCHITECTURE_IMPLEMENTATION.md step-by-step
2. Install packages for each microservice
3. Implement CQRS pattern
4. Test and iterate

**Time:** 10-15 hours (manual)
**Benefit:** Hands-on learning

## ?? Implementation Pattern (Per Microservice)

### Before (Current):
```
WMS.Inbound.API
??? Controllers
?   ??? InboundController.cs (calls IInboundService)
??? DTOs ? Already done
??? Interfaces ? Already done
??? Common\Models ? Already done
??? Program.cs

WMS.Infrastructure (Shared - BAD)
??? Services
    ??? InboundService.cs (business logic here)
```

### After (Target):
```
WMS.Inbound.API
??? Application (NEW)
?   ??? Commands
?   ?   ??? CreateInbound/
?   ?   ?   ??? CreateInboundCommand.cs
?   ?   ?   ??? CreateInboundCommandHandler.cs
?   ?   ?   ??? CreateInboundCommandValidator.cs
?   ?   ??? ReceiveInbound/
?   ?   ??? CancelInbound/
?   ??? Queries
?   ?   ??? GetInboundById/
?   ?   ?   ??? GetInboundByIdQuery.cs
?   ?   ?   ??? GetInboundByIdQueryHandler.cs
?   ?   ??? GetAllInbounds/
?   ??? Mappers
?       ??? InboundMapper.cs
??? Infrastructure (NEW - if needed)
?   ??? Data
?       ??? InboundQueries.cs
??? Controllers
?   ??? InboundController.cs (calls MediatR)
??? DTOs ? Already done
??? Interfaces ? Already done
??? Common\Models ? Already done
??? Program.cs (updated for MediatR)

WMS.Domain (Shared - GOOD)
??? Entities
??? Data
?   ??? WMSDbContext.cs ? Just moved here
??? Migrations
```

## ?? Key Changes Per Microservice

### 1. Install Packages
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

### 2. Update Program.cs
```csharp
// OLD
using WMS.Infrastructure.Data;
b => b.MigrationsAssembly("WMS.Infrastructure")

// NEW
using WMS.Domain.Data;
b => b.MigrationsAssembly("WMS.Domain")

// ADD
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

### 3. Create Application Layer
- Commands (Create, Update, Delete operations)
- Queries (Read operations)
- Handlers (Business logic)
- Validators (Input validation)
- Mappers (Entity ? DTO conversion)

### 4. Update Controller
```csharp
// OLD
private readonly IInboundService _service;
var result = await _service.CreateAsync(dto, currentUser);

// NEW
private readonly IMediator _mediator;
var command = new CreateInboundCommand { Dto = dto, CurrentUser = currentUser };
var result = await _mediator.Send(command);
```

## ?? Microservices to Refactor

| Microservice | Priority | Estimated Time |
|--------------|----------|----------------|
| WMS.Inbound.API | ? HIGH (Prototype) | 2-3 hours |
| WMS.Outbound.API | ? HIGH | 2 hours |
| WMS.Inventory.API | ? HIGH | 2-3 hours |
| WMS.Products.API | ?? MEDIUM | 1-2 hours |
| WMS.Locations.API | ?? MEDIUM | 1-2 hours |
| WMS.Payment.API | ?? LOW | 1-2 hours |
| WMS.Delivery.API | ?? LOW | 1-2 hours |
| WMS.Auth.API | ?? LOW | 1-2 hours |

**Total:** ~12-17 hours (manual) or ~3-4 hours (if I implement prototype)

## ?? Benefits

? **Independent Microservices** - Each can evolve separately  
? **Better Testing** - Handlers are easy to unit test  
? **CQRS Pattern** - Read/Write separation  
? **Clear Business Logic** - All in Application layer  
? **Shared Domain** - Single source of truth  
? **Centralized Migrations** - In WMS.Domain  

## ?? What Gets Removed

After refactoring:
- ? WMS.Application project (DTOs already moved)
- ? WMS.Infrastructure\Services (moved to microservices)
- ? WMS.Infrastructure\Repositories (kept - generic, reusable)
- ? WMS.Domain (enhanced with DbContext + Migrations)

## ?? Migration Commands (New)

```powershell
# Create migration
dotnet ef migrations add MigrationName --project WMS.Domain --startup-project WMS.Auth.API

# Apply migration
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API

# Remove migration
dotnet ef migrations remove --project WMS.Domain --startup-project WMS.Auth.API
```

## ?? Decision Time

**Ready to proceed?**

**Reply with:**
- **"A"** - You implement prototype for WMS.Inbound.API ?
- **"B"** - I'll implement manually following the guide

**Either way, let's make this happen! ??**

---

**Files:** 3 documentation files created  
**Build:** ? Successful  
**DbContext:** ? Moved to WMS.Domain  
**Status:** ? Awaiting your decision
