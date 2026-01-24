# Shared Database Architecture with WMS.Domain

## ?? Architecture Overview

Your WMS system uses a **Shared Database + Microservices** architecture pattern:

```
???????????????????????????????????????????????????????????????
?                    WMS.Gateway (API Gateway)                ?
?                      http://localhost:5000                  ?
???????????????????????????????????????????????????????????????
                       ?
       ?????????????????????????????????
       ?                               ?
???????????????                 ???????????????
? Microservice?                 ? Microservice?
?    APIs     ?  ?????????????? ?    APIs     ?
?  (8 APIs)   ?   Shared Domain ?  (8 APIs)   ?
???????????????                 ???????????????
       ?                               ?
       ?????????????????????????????????
                       ?
            ???????????????????????
            ?    WMS.Domain       ?
            ?  (Shared Layer)     ?
            ?  - DbContext        ?
            ?  - Entities         ?
            ?  - Repositories     ?
            ?  - Migrations       ?
            ???????????????????????
                       ?
            ???????????????????????
            ?   SQL Server        ?
            ?   WMSDatabase       ?
            ???????????????????????
```

## ?? Key Architectural Decision

**Pattern**: **Modular Monolith with Microservice APIs**

### ? Why This Pattern?

| Benefit | Explanation |
|---------|-------------|
| **Single Database** | All microservices share one database ? ACID transactions |
| **Shared Domain** | Entities, DbContext in WMS.Domain ? No duplication |
| **Microservice APIs** | Each API is independently deployable |
| **Flexibility** | Easy to split into separate databases later |
| **Simplicity** | Easier migrations, no distributed transactions |

### ?? Trade-offs

| Pro | Con |
|-----|-----|
| ? Strong consistency | ? Less service independence |
| ? Easy cross-entity queries | ? Single point of failure (DB) |
| ? No data duplication | ? Shared schema changes |
| ? Simple migrations | ? Harder to scale individually |

---

## ?? Project Structure

### **WMS.Domain** (Shared Data Layer)

```
WMS.Domain/
??? Common/
?   ??? BaseEntity.cs                    # Base entity with audit fields
?   ??? IAuditableEntity.cs              # Audit interface
??? Data/
?   ??? WMSDbContext.cs                  # ? SINGLE DbContext for ALL services
??? Entities/
?   ??? Product.cs
?   ??? Location.cs
?   ??? Inventory.cs
?   ??? Inbound.cs, InboundItem.cs
?   ??? Outbound.cs, OutboundItem.cs
?   ??? Payment.cs, PaymentEvent.cs
?   ??? Delivery.cs, DeliveryEvent.cs
?   ??? User.cs, Role.cs, UserRole.cs
??? Enums/
?   ??? Enums.cs                         # ProductStatus, InboundStatus, etc.
??? Interfaces/
?   ??? IRepository.cs
?   ??? IUnitOfWork.cs
??? Repositories/
?   ??? Repository.cs                    # Generic repository implementation
?   ??? UnitOfWork.cs                    # Transaction management
??? Migrations/
?   ??? 20260117063511_InitialCreate.cs  # ? EF Core migrations
?   ??? WMSDbContextModelSnapshot.cs
??? WMS.Domain.csproj                    # ? Has EF Core packages
```

### **Microservice APIs** (8 Independent APIs)

Each microservice:
- References `WMS.Domain` project
- Has its own **CQRS** (Commands/Queries)
- Has its own **Controllers**
- Shares the same `WMSDbContext`

```
WMS.Products.API/
??? Application/
?   ??? Commands/                        # Create, Update, Activate, Deactivate
?   ??? Queries/                         # GetById, GetAll, GetBySKU
?   ??? Mappers/                         # Entity ? DTO mapping
?   ??? Validators/                      # FluentValidation
??? Controllers/
?   ??? ProductsController.cs
??? DTOs/
?   ??? Product/
??? Common/
?   ??? Models/                          # Result, PagedResult
??? Program.cs                           # DbContext registration
```

---

## ?? WMS.Domain.csproj Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- ? Full EF Core packages for migrations -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

---

## ??? Database Migrations

### Creating Migrations

**Option 1: From Any Microservice API (Recommended)**

```powershell
# From WMS.Products.API
dotnet ef migrations add YourMigrationName `
    --project ../WMS.Domain `
    --startup-project . `
    --context WMSDbContext
```

**Option 2: From WMS.Domain with Startup Project**

```powershell
# From solution root
dotnet ef migrations add YourMigrationName `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --context WMSDbContext
```

### Applying Migrations

```powershell
# Update database
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

### Why This Works

- ? **WMS.Domain** has EF Core packages
- ? **WMS.Domain** has DbContext
- ? **Startup project** (any API) provides connection string
- ? Migrations stored in `WMS.Domain/Migrations/`

---

## ?? Microservice Configuration

### Program.cs (Each Microservice)

```csharp
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Interfaces;
using WMS.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ? Register shared DbContext
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));  // ? Migrations in Domain

// ? Register shared repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ? Register MediatR for CQRS
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ? Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

### appsettings.json (Each Microservice)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WMSDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

---

## ?? Cross-Service Communication

### Scenario: Outbound references Product

**Option 1: Navigation Properties (Current)**

```csharp
// Outbound.cs
public class Outbound
{
    public Guid Id { get; set; }
    // ... other properties
}

// OutboundItem.cs
public class OutboundItem
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;  // ? Navigation property
}
```

**Benefits**:
- ? Can use `.Include(o => o.Product)` in queries
- ? Single database query
- ? Strong typing

**Option 2: ID-Only References (Decoupled)**

```csharp
public class OutboundItem
{
    public Guid ProductId { get; set; }  // ? Just the ID
    // No navigation property
}

// In OutboundController - call Products API
var product = await _httpClient.GetAsync($"http://products-api/products/{productId}");
```

**Benefits**:
- ? Services are more independent
- ? Easier to split databases later
- ? Multiple HTTP calls needed

---

## ?? Current Services

| Service | Port | Entities | Responsibilities |
|---------|------|----------|------------------|
| **Auth.API** | 5001 | User, Role, UserRole | Authentication, Authorization |
| **Products.API** | 5002 | Product | Product catalog management |
| **Locations.API** | 5003 | Location | Warehouse location management |
| **Inventory.API** | 5006 | Inventory, InventoryTransaction | Stock management |
| **Inbound.API** | 5004 | Inbound, InboundItem | Receiving operations |
| **Outbound.API** | 5005 | Outbound, OutboundItem | Shipping operations |
| **Payment.API** | 5007 | Payment, PaymentEvent | Payment processing |
| **Delivery.API** | 5008 | Delivery, DeliveryEvent | Delivery tracking |

---

## ?? Running the System

### Start All Services

```powershell
# Use the provided script
.\run-all-services.ps1
```

### Or Start Individually

```powershell
# Terminal 1 - Gateway
cd WMS.Gateway
dotnet run

# Terminal 2 - Auth API
cd WMS.Auth.API
dotnet run

# Terminal 3 - Products API
cd WMS.Products.API
dotnet run

# ... and so on
```

### Access

- **API Gateway**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Individual APIs**: http://localhost:5001-5008

---

## ?? CQRS Pattern in Each Microservice

### Example: Create Product

```csharp
// 1. Command
public record CreateProductCommand(CreateProductDto Dto, string CurrentUser) 
    : IRequest<Result<ProductDto>>;

// 2. Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<ProductDto>> Handle(...)
    {
        var product = new Product { ... };
        await _productRepo.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return Result<ProductDto>.Success(ProductMapper.MapToDto(product));
    }
}

// 3. Controller
[HttpPost]
public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
{
    var command = new CreateProductCommand(dto, CurrentUser);
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}
```

---

## ??? Development Workflow

### Adding a New Entity

1. **Create Entity in WMS.Domain/Entities/**
```csharp
namespace WMS.Domain.Entities;

public class YourEntity : BaseEntity
{
    // Properties
}
```

2. **Add DbSet in WMSDbContext**
```csharp
public DbSet<YourEntity> YourEntities => Set<YourEntity>();
```

3. **Configure Entity in OnModelCreating**
```csharp
modelBuilder.Entity<YourEntity>(entity =>
{
    entity.HasKey(e => e.Id);
    // Configure properties and relationships
});
```

4. **Create Migration**
```powershell
dotnet ef migrations add AddYourEntity `
    --project WMS.Domain `
    --startup-project WMS.YourService.API
```

5. **Update Database**
```powershell
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.YourService.API
```

---

## ?? Best Practices

### ? DO

- ? Keep all entities in `WMS.Domain/Entities`
- ? Use `WMSDbContext` in all microservices
- ? Run migrations from `WMS.Domain`
- ? Use CQRS pattern in each microservice
- ? Use repository pattern for data access
- ? Use DTOs for API contracts
- ? Use `Result<T>` pattern for operation results

### ? DON'T

- ? Create separate DbContexts per microservice
- ? Duplicate entities across services
- ? Put business logic in entities
- ? Access DbContext directly in controllers
- ? Return entities from controllers (use DTOs)

---

## ?? Migration to Separate Databases (Future)

If you need to split into separate databases later:

### Step 1: Extract Entity Subset
```powershell
# Create new DbContext for Products
public class ProductsDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
}
```

### Step 2: Create Separate Database
```csharp
// In WMS.Products.API
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProductsConnection")));
```

### Step 3: Handle Cross-Database References
```csharp
// Remove navigation properties
public class OutboundItem
{
    public Guid ProductId { get; set; }  // Keep ID only
    // Remove: public Product Product { get; set; }
}

// Use HTTP calls or events
var product = await _httpClient.GetAsync($"/products/{productId}");
```

---

## ?? Summary

**Current Architecture**: Shared Database + Microservice APIs

**Pros**:
- ? Simple to develop and maintain
- ? ACID transactions across entities
- ? No data duplication
- ? Easy querying across entities
- ? Single migration path

**Future Path**:
- Can evolve to separate databases per service
- Can implement event-driven architecture
- Can add message queue (RabbitMQ, Kafka)

**This architecture is PERFECT for**:
- Teams learning microservices
- Applications with strong consistency requirements
- Systems that may need to scale later
- Projects with shared domain model

**You have successfully implemented a pragmatic, scalable architecture! ??**
