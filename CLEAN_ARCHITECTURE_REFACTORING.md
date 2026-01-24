# Clean Architecture Refactoring Guide

## ?? Objective
Refactor the WMS microservices solution to follow Clean Architecture principles where:
- Each microservice is independently deployable with its own Application layer
- Domain project is shared across all microservices (common entities)
- Infrastructure is split per microservice for data access
- Migrations are managed in the Domain project

---

## ?? Current vs Target Architecture

### Current Structure (Issues):
```
WMS.Infrastructure (Monolithic)
??? Services (All business logic)
?   ??? InboundService.cs
?   ??? OutboundService.cs
?   ??? InventoryService.cs
?   ??? ProductService.cs
?   ??? ... (all services)
??? Repositories (Generic)
??? Data (DbContext + Migrations)

WMS.Application (Monolithic)
??? Interfaces (All service interfaces)
??? DTOs (All DTOs - already moved)

Each Microservice API
??? Controllers
??? Program.cs
??? appsettings.json
```

### Target Structure (Clean Architecture per Microservice):
```
WMS.Domain (SHARED)
??? Entities (All domain entities)
??? Enums
??? Common
??? Interfaces (IRepository, IUnitOfWork)
??? Migrations (EF Core migrations)

WMS.Inbound.API
??? Application
?   ??? Commands
?   ?   ??? CreateInbound
?   ?   ?   ??? CreateInboundCommand.cs
?   ?   ?   ??? CreateInboundCommandHandler.cs
?   ?   ??? ReceiveInbound
?   ?   ??? CancelInbound
?   ??? Queries
?   ?   ??? GetInboundById
?   ?   ??? GetAllInbounds
?   ??? Services
?       ??? InboundService.cs
??? Infrastructure
?   ??? Data
?   ?   ??? InboundDbContext.cs (specific queries)
?   ??? Repositories
?       ??? InboundRepository.cs
??? DTOs (Already created)
??? Interfaces (Already created)
??? Common\Models (Already created)
??? Controllers
??? Program.cs

[Similar structure for each microservice]
```

---

## ?? Migration Strategy

### Phase 1: Domain Project Setup
- ? Move migrations from WMS.Infrastructure to WMS.Domain
- ? Update DbContext to be in WMS.Domain or shared location
- ? Configure all microservices to use shared DbContext

### Phase 2: Per-Microservice Application Layer
For each microservice (Inbound, Outbound, Inventory, etc.):
1. Create Application folder structure (Commands, Queries, Services)
2. Implement CQRS pattern with MediatR
3. Move service implementation from WMS.Infrastructure to microservice
4. Update dependency injection

### Phase 3: Per-Microservice Infrastructure
For each microservice:
1. Create Infrastructure folder for data access
2. Implement repository pattern specific to microservice needs
3. Configure DbContext usage

### Phase 4: Cleanup
1. Remove WMS.Application project (DTOs already moved)
2. Remove WMS.Infrastructure services
3. Keep WMS.Infrastructure only for shared utilities if needed

---

## ?? Implementation Steps

### Step 1: Install Required Packages

Each microservice will need:
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
```

### Step 2: Move DbContext and Migrations to Domain

**Create:** `WMS.Domain\Data\WMSDbContext.cs`
- Move from WMS.Infrastructure\Data\WMSDbContext.cs
- Keep all entity configurations

**Create:** `WMS.Domain\Migrations\`
- Move all migration files from WMS.Infrastructure\Migrations

**Update:** `WMS.Domain\WMS.Domain.csproj`
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Step 3: Implement CQRS for WMS.Inbound.API

**Create folder structure:**
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
?   ??? Services/
?       ??? InboundApplicationService.cs
??? Infrastructure/
?   ??? Data/
?   ?   ??? InboundQueries.cs
?   ??? Repositories/
?       ??? InboundRepository.cs
```

### Step 4: Example CQRS Implementation

**Command Example:**
```csharp
// Application/Commands/CreateInbound/CreateInboundCommand.cs
using MediatR;
using WMS.Inbound.API.DTOs.Inbound;
using WMS.Inbound.API.Common.Models;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommand : IRequest<Result<InboundDto>>
{
    public CreateInboundDto Dto { get; set; }
    public string CurrentUser { get; set; }
}
```

**Handler Example:**
```csharp
// Application/Commands/CreateInbound/CreateInboundCommandHandler.cs
using MediatR;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.DTOs.Inbound;
using WMS.Inbound.API.Common.Models;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommandHandler : IRequestHandler<CreateInboundCommand, Result<InboundDto>>
{
    private readonly IRepository<Inbound> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInboundCommandHandler(IRepository<Inbound> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(CreateInboundCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
        var inbound = new Inbound
        {
            // Map from request.Dto
        };

        await _repository.AddAsync(inbound);
        await _unitOfWork.SaveChangesAsync();

        return Result<InboundDto>.Success(MapToDto(inbound));
    }

    private InboundDto MapToDto(Inbound inbound)
    {
        // Mapping logic
        return new InboundDto();
    }
}
```

**Query Example:**
```csharp
// Application/Queries/GetInboundById/GetInboundByIdQuery.cs
using MediatR;
using WMS.Inbound.API.DTOs.Inbound;
using WMS.Inbound.API.Common.Models;

namespace WMS.Inbound.API.Application.Queries.GetInboundById;

public class GetInboundByIdQuery : IRequest<Result<InboundDto>>
{
    public Guid Id { get; set; }
}
```

### Step 5: Update Controller

```csharp
// Controllers/InboundController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WMS.Inbound.API.Application.Commands.CreateInbound;
using WMS.Inbound.API.Application.Queries.GetInboundById;

[ApiController]
[Route("api/[controller]")]
public class InboundController : ControllerBase
{
    private readonly IMediator _mediator;

    public InboundController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInboundByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(result.Errors);
            
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInboundDto dto)
    {
        var command = new CreateInboundCommand 
        { 
            Dto = dto, 
            CurrentUser = User.Identity?.Name ?? "System" 
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Errors);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }
}
```

### Step 6: Update Program.cs

```csharp
// Program.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration - Use shared DbContext from Domain
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));

// MediatR - Register all handlers from current assembly
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Repositories - Still use generic from Infrastructure
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JWT, CORS, etc. (existing configuration)
```

---

## ?? Migration Commands Update

After moving to WMS.Domain:

```powershell
# Create new migration
dotnet ef migrations add MigrationName --project WMS.Domain --startup-project WMS.Auth.API

# Update database
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API

# Remove migration
dotnet ef migrations remove --project WMS.Domain --startup-project WMS.Auth.API
```

---

## ?? Microservices to Refactor

Apply the same pattern to all microservices:

1. ? WMS.Inbound.API
2. ? WMS.Outbound.API
3. ? WMS.Inventory.API
4. ? WMS.Locations.API
5. ? WMS.Products.API
6. ? WMS.Payment.API
7. ? WMS.Delivery.API
8. ? WMS.Auth.API

---

## ?? Benefits of This Approach

### 1. **Independent Deployability**
- Each microservice has its own Application layer
- No shared WMS.Application dependency
- Can evolve independently

### 2. **Shared Domain**
- Single source of truth for entities
- Consistent business rules
- Centralized migrations

### 3. **Clean Architecture**
- Clear separation of concerns
- Testable business logic
- CQRS pattern for scalability

### 4. **Maintainability**
- Each microservice is self-contained
- Easy to understand and modify
- Clear dependencies

---

## ?? Implementation Priority

### High Priority (Core Operations):
1. WMS.Inbound.API
2. WMS.Outbound.API
3. WMS.Inventory.API

### Medium Priority:
4. WMS.Products.API
5. WMS.Locations.API

### Lower Priority:
6. WMS.Payment.API
7. WMS.Delivery.API
8. WMS.Auth.API (Already fairly independent)

---

## ?? Next Steps

1. Review this architecture plan
2. Confirm approach aligns with your vision
3. Start implementation with WMS.Inbound.API as prototype
4. Apply pattern to remaining microservices
5. Remove deprecated projects (WMS.Application, WMS.Infrastructure services)

Would you like me to start implementing this refactoring for WMS.Inbound.API as a prototype?
