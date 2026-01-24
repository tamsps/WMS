# Database Migration Quick Reference

## ?? TL;DR

**Your setup**: All microservices share `WMS.Domain` project with a single `WMSDbContext` and one database.

---

## ?? Migration Commands

### ? Create a New Migration

**From any microservice (Recommended)**:

```powershell
# Navigate to any API project
cd WMS.Products.API

# Create migration
dotnet ef migrations add YourMigrationName `
    --project ../WMS.Domain `
    --startup-project . `
    --context WMSDbContext

# Example:
dotnet ef migrations add AddProductCategory `
    --project ../WMS.Domain `
    --startup-project . `
    --context WMSDbContext
```

**From solution root**:

```powershell
dotnet ef migrations add YourMigrationName `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --context WMSDbContext
```

---

### ? Apply Migrations to Database

```powershell
# From any microservice
cd WMS.Products.API

dotnet ef database update `
    --project ../WMS.Domain `
    --startup-project .

# Or from solution root
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

---

### ? List All Migrations

```powershell
dotnet ef migrations list `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

---

### ? Remove Last Migration (if not applied)

```powershell
dotnet ef migrations remove `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

---

### ? Generate SQL Script

```powershell
# Generate SQL for all migrations
dotnet ef migrations script `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --output migration.sql

# Generate SQL for specific migration
dotnet ef migrations script `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --from 20260117063511_InitialCreate `
    --to 20260120000000_YourMigration `
    --output update.sql
```

---

### ? Reset Database

```powershell
# Drop database
dotnet ef database drop `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --force

# Recreate from migrations
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

---

## ?? Common Scenarios

### Scenario 1: Add a New Property to Existing Entity

1. **Modify the entity in `WMS.Domain/Entities/`**:
```csharp
public class Product : BaseEntity
{
    // Existing properties...
    public string? NewProperty { get; set; }  // ? Add this
}
```

2. **Create migration**:
```powershell
cd WMS.Products.API
dotnet ef migrations add AddProductNewProperty `
    --project ../WMS.Domain `
    --startup-project .
```

3. **Review generated migration** in `WMS.Domain/Migrations/`

4. **Apply to database**:
```powershell
dotnet ef database update --project ../WMS.Domain --startup-project .
```

---

### Scenario 2: Create a New Entity

1. **Create entity file** `WMS.Domain/Entities/NewEntity.cs`:
```csharp
namespace WMS.Domain.Entities;

public class NewEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    // Other properties...
}
```

2. **Add DbSet in `WMS.Domain/Data/WMSDbContext.cs`**:
```csharp
public DbSet<NewEntity> NewEntities => Set<NewEntity>();
```

3. **Configure entity in `OnModelCreating`**:
```csharp
modelBuilder.Entity<NewEntity>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
});
```

4. **Create and apply migration**:
```powershell
cd WMS.YourService.API
dotnet ef migrations add AddNewEntity --project ../WMS.Domain --startup-project .
dotnet ef database update --project ../WMS.Domain --startup-project .
```

---

### Scenario 3: Add a Relationship

1. **Update entities**:
```csharp
// Parent entity
public class Product : BaseEntity
{
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

// Child entity
public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;
}
```

2. **Configure relationship in DbContext**:
```csharp
modelBuilder.Entity<Review>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.HasOne(e => e.Product)
        .WithMany(p => p.Reviews)
        .HasForeignKey(e => e.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

3. **Create migration**:
```powershell
dotnet ef migrations add AddProductReviews `
    --project WMS.Domain `
    --startup-project WMS.Products.API
```

---

## ?? Common Issues

### Issue 1: "No DbContext was found"

**Error**:
```
Unable to create an object of type 'WMSDbContext'
```

**Solution**: Always specify startup project that has connection string:
```powershell
dotnet ef migrations add MyMigration `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API  # ? Must have appsettings.json
```

---

### Issue 2: "Build failed"

**Error**:
```
Build failed. Use dotnet build to see the errors.
```

**Solution**: Build solution first:
```powershell
dotnet build WMS.sln
# Then run migration command
```

---

### Issue 3: "A migration has already been applied"

**Error**:
```
The migration '20260117063511_InitialCreate' has already been applied to the database
```

**Solution**: This is normal. Your migration was already applied. Create a new migration for changes:
```powershell
dotnet ef migrations add YourNewMigration --project WMS.Domain --startup-project WMS.Inbound.API
```

---

### Issue 4: Wrong connection string

**Error**:
```
Login failed for user... / Cannot open database
```

**Solution**: Check `appsettings.json` in startup project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WMSDatabase;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

## ?? Migration Files Location

All migrations are stored in:
```
WMS.Domain/
??? Migrations/
    ??? 20260117063511_InitialCreate.cs
    ??? 20260117063511_InitialCreate.Designer.cs
    ??? [YourNewMigration].cs
    ??? [YourNewMigration].Designer.cs
    ??? WMSDbContextModelSnapshot.cs  ? Always updated
```

---

## ?? Best Practices

### ? DO

- ? Always specify `--project WMS.Domain`
- ? Always specify `--startup-project` (any API with connection string)
- ? Review generated migration code before applying
- ? Use descriptive migration names (`AddProductCategory` not `Update1`)
- ? Test migrations on development database first
- ? Keep migrations small and focused

### ? DON'T

- ? Manually edit `WMSDbContextModelSnapshot.cs`
- ? Delete migration files after they're applied to database
- ? Modify migration files after `dotnet ef database update`
- ? Run migrations from `WMS.Domain` without startup project

---

## ?? Quick Start Script

Create `add-migration.ps1` in solution root:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "Creating migration: $MigrationName" -ForegroundColor Green

dotnet ef migrations add $MigrationName `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API `
    --context WMSDbContext

Write-Host "`nMigration created successfully!" -ForegroundColor Green
Write-Host "To apply: dotnet ef database update --project WMS.Domain --startup-project WMS.Inbound.API" -ForegroundColor Yellow
```

**Usage**:
```powershell
.\add-migration.ps1 -MigrationName "AddProductCategory"
```

---

## ?? Current Database Schema

Your database `WMSDatabase` contains:

| Table | Purpose | Service |
|-------|---------|---------|
| Products | Product catalog | Products.API |
| Locations | Warehouse locations | Locations.API |
| Inventories | Stock levels | Inventory.API |
| InventoryTransactions | Stock movements | Inventory.API |
| Inbounds, InboundItems | Receiving | Inbound.API |
| Outbounds, OutboundItems | Shipping | Outbound.API |
| Payments, PaymentEvents | Payments | Payment.API |
| Deliveries, DeliveryEvents | Delivery tracking | Delivery.API |
| Users, Roles, UserRoles | Authentication | Auth.API |

**All tables share the same database connection** ?

---

## ?? Verify Current State

```powershell
# Check which migrations are applied
dotnet ef migrations list `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API

# Check database connection
dotnet ef dbcontext info `
    --project WMS.Domain `
    --startup-project WMS.Inbound.API
```

---

## ?? Summary

**Your Architecture**:
- ? One Domain project (`WMS.Domain`)
- ? One DbContext (`WMSDbContext`)  
- ? One Database (`WMSDatabase`)
- ? Migrations stored in `WMS.Domain/Migrations/`
- ? All microservices share the same database

**Migration Workflow**:
1. Modify entities in `WMS.Domain/Entities/`
2. Update `WMSDbContext` if needed
3. Run `dotnet ef migrations add` from any API
4. Review generated migration
5. Run `dotnet ef database update`
6. All microservices see the changes ?

**Need help?** See `SHARED_DATABASE_ARCHITECTURE.md` for complete documentation.
