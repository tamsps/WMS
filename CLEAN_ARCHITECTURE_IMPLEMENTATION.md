# ?? Clean Architecture Implementation Guide

## Current Status

? **Phase 1 Completed:**
- DbContext moved to WMS.Domain (WMS.Domain\Data\WMSDbContext.cs)
- EF Core packages added to WMS.Domain.csproj
- DTOs and Interfaces already moved to each microservice

? **Next Steps:**
Follow this guide to implement Clean Architecture for each microservice.

---

## ?? Implementation Checklist

### Prerequisites
- [ ] Build the solution to ensure no errors
- [ ] Backup your database (WMSDB)
- [ ] Create a new git branch for this refactoring

---

## ?? Step-by-Step Implementation

### STEP 1: Install Required NuGet Packages

For **each microservice** (Inbound, Outbound, Inventory, Locations, Products, Payment, Delivery, Auth), install:

```powershell
# Navigate to each API project directory
cd WMS.Inbound.API

# Install MediatR for CQRS
dotnet add package MediatR --version 12.4.1
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection --version 11.1.0

# Install FluentValidation
dotnet add package FluentValidation --version 11.11.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.11.0

# Repeat for all microservices
```

**Or update each .csproj manually:**

```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.4.1" />
  <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
  <PackageReference Include="FluentValidation" Version="11.11.0" />
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
</ItemGroup>
```

---

### STEP 2: Update All Microservices to Use WMS.Domain.Data.WMSDbContext

**Update each microservice's Program.cs:**

**Before:**
```csharp
using WMS.Infrastructure.Data;  // OLD

builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Infrastructure")));  // OLD
```

**After:**
```csharp
using WMS.Domain.Data;  // NEW

builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));  // NEW - Migrations now in Domain
```

**Files to Update:**
- [ ] WMS.Inbound.API\Program.cs
- [ ] WMS.Outbound.API\Program.cs
- [ ] WMS.Inventory.API\Program.cs
- [ ] WMS.Locations.API\Program.cs
- [ ] WMS.Products.API\Program.cs
- [ ] WMS.Payment.API\Program.cs
- [ ] WMS.Delivery.API\Program.cs
- [ ] WMS.Auth.API\Program.cs

---

### STEP 3: Update WMS.Infrastructure References

**Update WMS.Infrastructure to use WMS.Domain.Data:**

Files to update in WMS.Infrastructure:
- [ ] WMS.Infrastructure\Repositories\Repository.cs
- [ ] WMS.Infrastructure\Repositories\UnitOfWork.cs
- [ ] All Service files (if not yet moved)

**Example for Repository.cs:**
```csharp
using WMS.Domain.Data;  // Change from WMS.Infrastructure.Data

namespace WMS.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly WMSDbContext _context;  // Still works, just different namespace
    // ...
}
```

---

### STEP 4: Create Migration in WMS.Domain

Since migrations are now in WMS.Domain, create a new migration to sync:

```powershell
# From solution root
dotnet ef migrations add MoveToD omainDbContext --project WMS.Domain --startup-project WMS.Auth.API

# Apply migration
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API
```

**Note:** This might create an empty migration since we're just moving the DbContext. That's okay!

---

### STEP 5: Prototype - Implement Clean Architecture for WMS.Inbound.API

I'll provide the complete implementation files for WMS.Inbound.API as a prototype. You can then replicate this pattern for other microservices.

---

## ?? WMS.Inbound.API Clean Architecture Structure

### File Structure:
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
??? Infrastructure/
?   ??? Data/
?   ?   ??? InboundQueries.cs
?   ??? Repositories/ (if needed)
??? DTOs/ (Already exists)
??? Interfaces/ (Already exists)
??? Common/Models/ (Already exists)
??? Controllers/
?   ??? InboundController.cs (Update to use MediatR)
??? Program.cs (Update for MediatR)
```

---

## ?? Implementation Files

I'll create implementation files in the next response. This is a large refactoring, so I'll provide:

1. ? Complete CQRS implementation for WMS.Inbound.API
2. ? Updated Program.cs with MediatR registration
3. ? Updated Controller to use MediatR
4. ? Mapper for DTO conversions
5. ? Validators for commands

Then you can replicate this pattern for:
- WMS.Outbound.API
- WMS.Inventory.API
- WMS.Locations.API
- WMS.Products.API
- WMS.Payment.API
- WMS.Delivery.API
- WMS.Auth.API

---

## ?? Important Notes

### Database Migrations
- Migrations are now in WMS.Domain project
- Use `--project WMS.Domain` for all migration commands
- All microservices share the same database and DbContext

### Shared vs Isolated
- **Shared:** WMS.Domain (entities, DbContext, interfaces)
- **Shared:** WMS.Infrastructure (generic repositories, UnitOfWork)
- **Isolated:** Each microservice has its own Application layer

### Benefits
1. Each microservice can evolve independently
2. CQRS provides better separation of read/write operations
3. Easier to test and maintain
4. Clear business logic encapsulation

---

## ?? Next Actions

1. **Review this guide**
2. **Confirm you want to proceed with this architecture**
3. **I'll implement the complete prototype for WMS.Inbound.API**
4. **You can then replicate for other microservices**

Would you like me to proceed with implementing the complete CQRS prototype for WMS.Inbound.API?
