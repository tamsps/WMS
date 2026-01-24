# ?? WMS.Web Migration Quick Reference

## ? Your New Setup

```
Startup Project:     WMS.Web (Razor Pages Frontend)
Data Project:        WMS.Domain (DbContext + Entities)
Connection String:   WMS.Web/appsettings.json
Migrations Folder:   WMS.Domain/Migrations/
```

---

## ?? Quick Commands

### From Solution Root (F:\PROJECT\STUDY\VMS\)

```powershell
# Create database (first time)
.\migrate.ps1 -Action update

# Add new migration
.\migrate.ps1 -Action add -MigrationName "YourMigrationName"

# Apply pending migrations
.\migrate.ps1 -Action update

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

## ?? Manual Commands (Without Script)

```powershell
# From solution root

# Create migration
dotnet ef migrations add YourMigrationName `
    --project WMS.Domain `
    --startup-project WMS.Web `
    --context WMSDbContext

# Apply to database
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Web

# List migrations
dotnet ef migrations list `
    --project WMS.Domain `
    --startup-project WMS.Web

# Remove migration
dotnet ef migrations remove `
    --project WMS.Domain `
    --startup-project WMS.Web

# Generate SQL script
dotnet ef migrations script `
    --project WMS.Domain `
    --startup-project WMS.Web `
    --output migration.sql
```

---

## ?? Connection String

**Location**: `WMS.Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

---

## ?? WMS.Web Configuration

### Required Packages (WMS.Web.csproj)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
```

### DbContext Registration (Program.cs)

```csharp
// ? Required for migrations
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));
```

---

## ?? What Gets Created

When you run `.\migrate.ps1 -Action update`:

? **Database**: `WMSDB` on `CONGTAM-PC`  
? **15 Tables**: All entities from WMS.Domain  
? **Seed Data**: Admin user + 3 roles  
? **Relationships**: All foreign keys configured  
? **Indexes**: Unique constraints on SKU, Email, etc.

### Default Login

```
Username: admin
Password: Admin@123
```

---

## ?? Verify Setup

```powershell
# Check database info
dotnet ef dbcontext info --project WMS.Domain --startup-project WMS.Web

# List applied migrations
dotnet ef migrations list --project WMS.Domain --startup-project WMS.Web

# Test database connection (start WMS.Web)
cd WMS.Web
dotnet run
# Navigate to http://localhost:5000
```

---

## ??? Troubleshooting

### Error: "Unable to create DbContext"

**Solution**: Install EF Core packages in WMS.Web
```powershell
cd WMS.Web
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Error: "No connection string found"

**Solution**: Verify `WMS.Web/appsettings.json` has `ConnectionStrings:DefaultConnection`

### Error: "Project does not reference WMS.Domain"

**Solution**: Add project reference
```powershell
cd WMS.Web
dotnet add reference ..\WMS.Domain\WMS.Domain.csproj
```

### Build Fails

**Solution**: Restore packages
```powershell
dotnet restore WMS.Web
dotnet build WMS.Web
```

---

## ?? Common Workflows

### 1. First Time Setup

```powershell
# Create database
.\migrate.ps1 -Action update

# Verify
.\migrate.ps1 -Action list

# Start app
cd WMS.Web
dotnet run
```

### 2. Add New Feature

```powershell
# 1. Modify entity in WMS.Domain/Entities/
# 2. Create migration
.\migrate.ps1 -Action add -MigrationName "AddNewFeature"

# 3. Review migration file in WMS.Domain/Migrations/
# 4. Apply to database
.\migrate.ps1 -Action update
```

### 3. Reset Database

```powershell
# Drop database
.\migrate.ps1 -Action drop

# Recreate from scratch
.\migrate.ps1 -Action update
```

---

## ?? Architecture Benefits

### Why WMS.Web as Startup?

| Benefit | Description |
|---------|-------------|
| **Centralized Config** | Single `appsettings.json` for connection string |
| **Frontend/DB Sync** | Web app and database always in sync |
| **Simpler Deployment** | Frontend includes DB setup |
| **Natural for Razor Pages** | Server-rendered app with direct DB access |
| **Easier Development** | One project to configure for migrations |

### Project Structure

```
WMS.Web (Frontend + Migration Startup)
  ??? appsettings.json          ? Connection string
  ??? Program.cs                ? DbContext registration
  ??? Controllers/              ? MVC controllers
  ??? Pages/                    ? Razor Pages
  ??? WMS.Web.csproj            ? EF Core packages

WMS.Domain (Shared Data Layer)
  ??? Data/WMSDbContext.cs      ? DbContext
  ??? Entities/                 ? All entities
  ??? Repositories/             ? Repository pattern
  ??? Migrations/               ? EF migrations

WMS.*.API (Microservices)
  ??? Application/              ? CQRS
  ??? Controllers/              ? REST APIs
  ??? appsettings.json          ? No longer need connection string
```

---

## ?? Summary

**You've successfully configured WMS.Web as your migration startup project!**

### Quick Start

```powershell
# 1. Create database
.\migrate.ps1 -Action update

# 2. Start frontend
cd WMS.Web
dotnet run

# 3. Login with admin/Admin@123
# 4. Start building features!
```

### Key Points

- ? Migrations run from WMS.Web
- ? Connection string in one place
- ? Frontend and DB always in sync
- ? Use `.\migrate.ps1` for all migrations
- ? All microservices still work independently

**Happy coding! ??**
