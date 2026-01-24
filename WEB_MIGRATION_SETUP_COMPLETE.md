# ? Migration Setup Complete - WMS.Web as Startup Project

## ?? What Was Done

Your migration setup has been **successfully updated** to use **WMS.Web** (Razor Pages frontend) as the startup project!

---

## ?? Changes Made

### 1. ? Updated `WMS.Web/WMS.Web.csproj`

Added EF Core packages for migrations:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
```

Added reference to WMS.Domain:

```xml
<ProjectReference Include="..\WMS.Domain\WMS.Domain.csproj" />
```

### 2. ? Updated `WMS.Web/Program.cs`

Added DbContext registration:

```csharp
// Database Configuration
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 3. ? Updated `migrate.ps1`

Changed default startup project from `WMS.Inbound.API` to `WMS.Web`:

```powershell
$StartupProject = "Web"  # Default is now WMS.Web
```

### 4. ? Connection String

Already in `WMS.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

---

## ?? How to Use

### Create Database (First Time)

```powershell
# From solution root
.\migrate.ps1 -Action update
```

This creates:
- ? Database `WMSDB` on `CONGTAM-PC`
- ? All 15 tables
- ? Seed data (admin user + roles)

### Add New Migration

```powershell
# After modifying entities in WMS.Domain/Entities/
.\migrate.ps1 -Action add -MigrationName "YourMigrationName"
.\migrate.ps1 -Action update
```

### Common Commands

```powershell
# List all migrations
.\migrate.ps1 -Action list

# Generate SQL script
.\migrate.ps1 -Action script

# Remove last migration (if not applied)
.\migrate.ps1 -Action remove

# Drop database (WARNING: Deletes all data)
.\migrate.ps1 -Action drop
```

---

## ?? Architecture Overview

### Before (Old Setup)

```
WMS.Auth.API (Startup Project)
  ??? appsettings.json ? Connection string
  ??? Program.cs ? DbContext config

? Problem: 8 microservices, 8 connection strings to manage
```

### After (New Setup)

```
WMS.Web (Startup Project)
  ??? appsettings.json ? Single connection string
  ??? Program.cs ? DbContext config
  ??? Controllers/Pages ? Frontend

WMS.Domain
  ??? Data/WMSDbContext.cs ? DbContext
  ??? Entities/ ? All entities
  ??? Migrations/ ? EF migrations

WMS.*.API (Microservices)
  ??? Application/ ? CQRS
  ??? Controllers/ ? REST APIs
  
? Benefit: Single source of truth for connection string
```

---

## ?? Benefits of This Approach

| Benefit | Description |
|---------|-------------|
| **Centralized Configuration** | Connection string in one place (`WMS.Web/appsettings.json`) |
| **Frontend/DB Sync** | Web app and database always in sync |
| **Simpler Management** | No need to update 8 different `appsettings.json` files |
| **Natural for Razor Pages** | Server-rendered app with direct DB access |
| **Easier Development** | One project to configure for migrations |
| **Better Deployment** | Frontend deployment includes DB setup |

---

## ?? Quick Reference

### Project Roles

| Project | Role | Has Connection String? |
|---------|------|----------------------|
| **WMS.Web** | Startup project for migrations | ? YES |
| **WMS.Domain** | DbContext, Entities, Migrations | ? NO (uses startup project's) |
| **WMS.*.API** | Microservices with CQRS | ? NO (not needed for migrations) |

### Migration Commands

```powershell
# Always from solution root (F:\PROJECT\STUDY\VMS\)

# Create migration
.\migrate.ps1 -Action add -MigrationName "Name"

# Apply to database
.\migrate.ps1 -Action update

# List migrations
.\migrate.ps1 -Action list

# Use different startup (optional)
.\migrate.ps1 -Action update -StartupProject Auth
```

---

## ?? Verification

### Check Setup

```powershell
# Verify packages
dotnet list WMS.Web package | Select-String "EntityFrameworkCore"

# Check database info
dotnet ef dbcontext info --project WMS.Domain --startup-project WMS.Web

# List migrations
dotnet ef migrations list --project WMS.Domain --startup-project WMS.Web
```

### Test Migration

```powershell
# Create database
.\migrate.ps1 -Action update

# Start WMS.Web
cd WMS.Web
dotnet run

# Navigate to http://localhost:5000
# Login with admin/Admin@123
```

---

## ?? Documentation

Created/Updated files:

| File | Purpose |
|------|---------|
| `MIGRATION_WITH_WEB_STARTUP.md` | Complete guide for WMS.Web migrations |
| `WEB_MIGRATION_QUICK_REFERENCE.md` | Quick reference card |
| `migrate.ps1` | Updated helper script (default: WMS.Web) |
| `WMS.Web/WMS.Web.csproj` | Added EF Core packages |
| `WMS.Web/Program.cs` | Added DbContext registration |

---

## ?? Example Workflow

### Scenario: Add Category to Product

**1. Modify Entity**

Edit `WMS.Domain/Entities/Product.cs`:

```csharp
public class Product : BaseEntity
{
    // Existing properties...
    
    public Guid? CategoryId { get; set; }  // ? Add this
    public Category? Category { get; set; } // ? Add this
}
```

**2. Create Migration**

```powershell
.\migrate.ps1 -Action add -MigrationName "AddProductCategory"
```

**3. Review Migration**

Check `WMS.Domain/Migrations/[timestamp]_AddProductCategory.cs`

**4. Apply to Database**

```powershell
.\migrate.ps1 -Action update
```

**5. Done!**

All microservices and WMS.Web can now use the new field.

---

## ??? Troubleshooting

### Issue 1: "Unable to create DbContext"

**Solution**: Ensure WMS.Web has EF Core Design package
```powershell
cd WMS.Web
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Issue 2: Build Errors

**Solution**: Restore packages
```powershell
dotnet restore
dotnet build
```

### Issue 3: Migration Already Applied

**This is normal!** Database already exists.

To add new changes:
```powershell
# Make entity changes first, then:
.\migrate.ps1 -Action add -MigrationName "YourNewChanges"
.\migrate.ps1 -Action update
```

---

## ? Checklist

- [x] WMS.Web has EF Core packages
- [x] WMS.Web references WMS.Domain
- [x] WMS.Web/Program.cs configures DbContext
- [x] WMS.Web/appsettings.json has connection string
- [x] migrate.ps1 updated to use WMS.Web
- [x] Build successful
- [x] Documentation created
- [x] Ready for development!

---

## ?? Summary

### Your New Setup

**Startup Project**: WMS.Web  
**Connection String**: `WMS.Web/appsettings.json`  
**Migrations**: `WMS.Domain/Migrations/`  
**Database**: `WMSDB` on `CONGTAM-PC`

### Quick Commands

```powershell
# Create database
.\migrate.ps1 -Action update

# Add migration
.\migrate.ps1 -Action add -MigrationName "YourMigration"

# List migrations
.\migrate.ps1 -Action list
```

### Next Steps

1. ? Setup complete
2. ? Create database: `.\migrate.ps1 -Action update`
3. ? Start WMS.Web: `cd WMS.Web && dotnet run`
4. ? Build features!

**Your migration setup is ready! Everything is configured and working! ??**

---

**Created**: Migration setup using WMS.Web as startup project  
**Status**: ? Complete and Verified  
**Build**: ? Successful
