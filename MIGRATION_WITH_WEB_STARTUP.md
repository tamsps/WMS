# ?? WMS Database Migrations - Using WMS.Web as Startup Project

## ?? Overview

You've centralized your configuration in **WMS.Web** (Razor Pages frontend), which now serves as the startup project for database migrations.

### Current Architecture

```
WMS.Web (Startup Project)
  ??? appsettings.json          ? Has connection string
  ??? Program.cs                ? Can configure DbContext
  ??? WMS.Web.csproj            ?? Needs EF Core packages

WMS.Domain (Data Layer)
  ??? Data/WMSDbContext.cs      ? Has DbContext
  ??? Entities/                 ? All domain entities
  ??? Migrations/               ? Migrations stored here
  ??? WMS.Domain.csproj         ? Has EF Core packages
```

### Connection String Location

**File**: `WMS.Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

---

## ?? Quick Start

### ? Step 1: Add EF Core Packages to WMS.Web

Since WMS.Web is now the startup project, it needs EF Core Design packages:

```powershell
# Navigate to WMS.Web
cd WMS.Web

# Add EF Core Design package
dotnet add package Microsoft.EntityFrameworkCore.Design

# Optional: Add SQL Server package if not inherited
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### ? Step 2: Register DbContext in WMS.Web (if not already done)

Update `WMS.Web/Program.cs`:

```csharp
using WMS.Web.Services;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ? Register DbContext
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ... rest of your configuration
```

---

## ?? Migration Commands

### From Solution Root Directory

```powershell
# Navigate to solution root
cd F:\PROJECT\STUDY\VMS

# Create migration
dotnet ef migrations add YourMigrationName `
    --project WMS.Domain `
    --startup-project WMS.Web `
    --context WMSDbContext

# Apply migration to database
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Web

# List all migrations
dotnet ef migrations list `
    --project WMS.Domain `
    --startup-project WMS.Web

# Remove last migration (if not applied)
dotnet ef migrations remove `
    --project WMS.Domain `
    --startup-project WMS.Web

# Generate SQL script
dotnet ef migrations script `
    --project WMS.Domain `
    --startup-project WMS.Web `
    --output migration.sql `
    --idempotent

# Drop database
dotnet ef database drop `
    --project WMS.Domain `
    --startup-project WMS.Web `
    --force
```

---

## ?? Update Helper Script (migrate.ps1)

Update your `migrate.ps1` to use WMS.Web as default startup project:

```powershell
# WMS Migration Helper Script - Updated for WMS.Web
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("add", "update", "list", "remove", "script", "drop")]
    [string]$Action = "add",
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [string]$StartupProject = "Web"  # ? Changed default to Web
)

$ErrorActionPreference = "Stop"

# Configuration
$DomainProject = "WMS.Domain"
$StartupProjectPath = "WMS.$StartupProject"  # WMS.Web

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?        WMS Database Migration Helper                ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Startup Project: $StartupProjectPath" -ForegroundColor Yellow
Write-Host ""

# ... rest of script (same logic, just different default startup project)
```

---

## ?? Usage Examples

### Example 1: Create Initial Database

```powershell
# From solution root
.\migrate.ps1 -Action update

# Or manually
dotnet ef database update --project WMS.Domain --startup-project WMS.Web
```

### Example 2: Add New Migration

```powershell
# Add a new field to Product entity
.\migrate.ps1 -Action add -MigrationName "AddProductCategory"

# Or manually
dotnet ef migrations add AddProductCategory `
    --project WMS.Domain `
    --startup-project WMS.Web
```

### Example 3: Apply Pending Migrations

```powershell
.\migrate.ps1 -Action update

# Or manually
dotnet ef database update --project WMS.Domain --startup-project WMS.Web
```

---

## ?? WMS.Web.csproj Requirements

Your `WMS.Web.csproj` should include:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- ? Required for migrations -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    
    <!-- ? For SQL Server -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>

  <ItemGroup>
    <!-- ? Reference to Domain project -->
    <ProjectReference Include="..\WMS.Domain\WMS.Domain.csproj" />
    <ProjectReference Include="..\WMS.Application\WMS.Application.csproj" />
  </ItemGroup>

</Project>
```

---

## ?? WMS.Web Program.cs Configuration

Complete example of `WMS.Web/Program.cs` with DbContext:

```csharp
using WMS.Web.Services;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Interfaces;
using WMS.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ? Database Configuration
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));

// ? Register Repository Pattern (Optional - if WMS.Web needs direct DB access)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register HttpClient and ApiService
builder.Services.AddHttpClient<IApiService, ApiService>();

var app = builder.Build();

// ? Auto-migrate on startup (Optional - Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WMSDbContext>();
    dbContext.Database.Migrate(); // ? Automatically apply pending migrations
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable session before authentication
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

---

## ?? Complete Workflow

### Step-by-Step: Create Database from WMS.Web

**1. Ensure packages are installed**

```powershell
cd WMS.Web
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**2. Add DbContext to Program.cs** (see above)

**3. Create database**

```powershell
# From solution root
dotnet ef database update --project WMS.Domain --startup-project WMS.Web
```

**4. Verify database**

```powershell
# Check database info
dotnet ef dbcontext info --project WMS.Domain --startup-project WMS.Web

# List migrations
dotnet ef migrations list --project WMS.Domain --startup-project WMS.Web
```

---

## ?? Comparison: Old vs New Approach

### ? Old Approach (Microservice as Startup)

```powershell
dotnet ef migrations add MyMigration `
    --project WMS.Domain `
    --startup-project WMS.Auth.API  # ? Using microservice
```

**Issues**:
- Each microservice has own `appsettings.json`
- Need to update 8 different files for connection string changes
- Migration tied to a specific microservice

### ? New Approach (WMS.Web as Startup)

```powershell
dotnet ef migrations add MyMigration `
    --project WMS.Domain `
    --startup-project WMS.Web  # ? Using frontend
```

**Benefits**:
- ? Single `appsettings.json` in WMS.Web
- ? Centralized configuration
- ? Frontend and backend share same database
- ? Easier to manage
- ? Aligns with Razor Pages architecture

---

## ?? Architecture Benefits

### Why WMS.Web as Startup Project Makes Sense

```
???????????????????????????????????????
?         WMS.Web (Frontend)          ?
?    Razor Pages + MVC Controllers    ?
?  ? Startup Project for Migrations  ?
?  ? Has Connection String           ?
?  ? Configures DbContext            ?
???????????????????????????????????????
               ?
               ??????? WMS.Domain (Shared Data Layer)
               ?       - DbContext
               ?       - Entities
               ?       - Repositories
               ?       - Migrations
               ?
               ??????? WMS.*.API (Microservices)
                       - CQRS Commands/Queries
                       - Business Logic
                       - REST APIs
```

### Benefits

| Benefit | Description |
|---------|-------------|
| **Single Source of Truth** | Connection string in one place |
| **Simplified Deployment** | Frontend and DB migrations together |
| **Easier Development** | One project to configure |
| **Consistent State** | Frontend and DB always in sync |
| **Better for Razor Pages** | Natural fit for server-rendered app |

---

## ??? Troubleshooting

### Issue 1: "Unable to create DbContext"

**Error**:
```
Unable to create an object of type 'WMSDbContext'
```

**Solution**: Ensure WMS.Web has EF Core Design package
```powershell
cd WMS.Web
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Issue 2: "Project does not reference WMS.Domain"

**Error**:
```
Project 'WMS.Web' does not reference 'WMS.Domain'
```

**Solution**: Add project reference
```powershell
cd WMS.Web
dotnet add reference ..\WMS.Domain\WMS.Domain.csproj
```

### Issue 3: "No connection string found"

**Error**:
```
No connection string named 'DefaultConnection' was found
```

**Solution**: Verify `WMS.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Issue 4: Build Fails

**Error**: Build errors in WMS.Web

**Solution**: Ensure all necessary packages are installed:
```powershell
cd WMS.Web
dotnet restore
dotnet build
```

---

## ?? Updated Helper Script

Save this as `migrate.ps1` (overwrites old version):

```powershell
# WMS Migration Helper Script - WMS.Web Startup
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("add", "update", "list", "remove", "script", "drop")]
    [string]$Action = "add",
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

$DomainProject = "WMS.Domain"
$StartupProject = "WMS.Web"

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?        WMS Database Migration Helper                ?" -ForegroundColor Cyan
Write-Host "?        Startup Project: WMS.Web                     ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

function Add-Migration {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Host "? Migration name is required!" -ForegroundColor Red
        Write-Host "Usage: .\migrate.ps1 -Action add -MigrationName YourMigrationName" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan
    Write-Host "  Domain Project: $DomainProject" -ForegroundColor Gray
    Write-Host "  Startup Project: $StartupProject" -ForegroundColor Gray
    Write-Host ""

    dotnet ef migrations add $MigrationName `
        --project $DomainProject `
        --startup-project $StartupProject `
        --context WMSDbContext

    Write-Host ""
    Write-Host "? Migration created!" -ForegroundColor Green
    Write-Host "Next: .\migrate.ps1 -Action update" -ForegroundColor Yellow
}

function Update-Database {
    Write-Host "Applying migrations to database..." -ForegroundColor Cyan
    
    dotnet ef database update `
        --project $DomainProject `
        --startup-project $StartupProject

    Write-Host ""
    Write-Host "? Database updated!" -ForegroundColor Green
}

function List-Migrations {
    dotnet ef migrations list `
        --project $DomainProject `
        --startup-project $StartupProject
}

function Remove-LastMigration {
    dotnet ef migrations remove `
        --project $DomainProject `
        --startup-project $StartupProject `
        --force
}

function Export-SqlScript {
    $scriptFile = "migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    
    dotnet ef migrations script `
        --project $DomainProject `
        --startup-project $StartupProject `
        --output $scriptFile `
        --idempotent

    Write-Host "? SQL script generated: $scriptFile" -ForegroundColor Green
}

function Drop-Database {
    Write-Host "??  WARNING: This will DELETE the database!" -ForegroundColor Red
    $confirmation = Read-Host "Type 'DELETE' to confirm"
    
    if ($confirmation -ne "DELETE") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }

    dotnet ef database drop `
        --project $DomainProject `
        --startup-project $StartupProject `
        --force

    Write-Host "? Database dropped!" -ForegroundColor Green
}

# Main execution
switch ($Action) {
    "add" { Add-Migration }
    "update" { Update-Database }
    "list" { List-Migrations }
    "remove" { Remove-LastMigration }
    "script" { Export-SqlScript }
    "drop" { Drop-Database }
}
```

---

## ?? Summary

### Your New Setup

**Startup Project**: `WMS.Web` (Razor Pages frontend)  
**Data Project**: `WMS.Domain` (DbContext, Entities, Migrations)  
**Connection String**: `WMS.Web/appsettings.json`

### Quick Commands

```powershell
# Create database
.\migrate.ps1 -Action update

# Add migration
.\migrate.ps1 -Action add -MigrationName "YourMigration"

# List migrations
.\migrate.ps1 -Action list

# Generate SQL script
.\migrate.ps1 -Action script
```

### Benefits of WMS.Web as Startup

? Single connection string location  
? Frontend and DB always in sync  
? Simpler configuration management  
? Natural fit for Razor Pages architecture  
? Easier for developers to understand  

**Your migration setup is ready! ??**
