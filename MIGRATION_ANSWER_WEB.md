# ?? Quick Answer: Migrations with WMS.Web

## ? Yes! Use These Commands

### From Solution Root (F:\PROJECT\STUDY\VMS\)

```powershell
# Create database
.\migrate.ps1 -Action update

# Add new migration
.\migrate.ps1 -Action add -MigrationName "YourMigrationName"

# Apply migrations
.\migrate.ps1 -Action update

# List migrations
.\migrate.ps1 -Action list
```

---

## ?? What Changed

### Before (Your Old Question)

```powershell
# You asked about using Auth.API
dotnet ef migrations add Name --project WMS.Domain --startup-project WMS.Auth.API
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API
```

### Now (Your New Setup - WMS.Web)

```powershell
# Now using WMS.Web (default in migrate.ps1)
dotnet ef migrations add Name --project WMS.Domain --startup-project WMS.Web
dotnet ef database update --project WMS.Domain --startup-project WMS.Web

# Or just use the script (easier):
.\migrate.ps1 -Action add -MigrationName "Name"
.\migrate.ps1 -Action update
```

---

## ?? Key Information

### Connection String Location

**File**: `WMS.Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Projects Involved

```
WMS.Web
  ??? appsettings.json          ? Connection string
  ??? Program.cs                ? DbContext registration
  ??? WMS.Web.csproj            ? EF Core packages

WMS.Domain
  ??? Data/WMSDbContext.cs      ? DbContext definition
  ??? Entities/                 ? All entities
  ??? Migrations/               ? Migration files
```

---

## ?? Complete Example

### Create Database from Scratch

```powershell
# Step 1: Navigate to solution root
cd F:\PROJECT\STUDY\VMS

# Step 2: Create database
.\migrate.ps1 -Action update

# Step 3: Verify
.\migrate.ps1 -Action list

# Step 4: Start WMS.Web
cd WMS.Web
dotnet run

# Step 5: Login
# Navigate to http://localhost:5000
# Username: admin
# Password: Admin@123
```

---

## ?? What Gets Created

When you run `.\migrate.ps1 -Action update`:

? **Database**: `WMSDB` on `CONGTAM-PC`  
? **15 Tables**: All entities from WMS.Domain/Entities/  
? **Seed Data**:
- Users: `admin` (password: `Admin@123`)
- Roles: Admin, Manager, WarehouseStaff

---

## ?? Common Tasks

```powershell
# Create database (first time)
.\migrate.ps1 -Action update

# Add new field to entity
# 1. Edit WMS.Domain/Entities/YourEntity.cs
# 2. Create migration
.\migrate.ps1 -Action add -MigrationName "AddNewField"
# 3. Apply to database
.\migrate.ps1 -Action update

# Reset database
.\migrate.ps1 -Action drop
.\migrate.ps1 -Action update

# Generate SQL script for production
.\migrate.ps1 -Action script
```

---

## ?? Documentation

| Document | Purpose |
|----------|---------|
| `MIGRATION_WITH_WEB_STARTUP.md` | Complete migration guide |
| `WEB_MIGRATION_QUICK_REFERENCE.md` | Quick command reference |
| `WEB_MIGRATION_SETUP_COMPLETE.md` | Setup summary |
| `migrate.ps1` | Helper script (updated) |

---

## ? Summary

### Your Question
> Could I run `dotnet ef migrations add name` and `dotnet ef database update` to create database in Domain project folder?

### Answer

**YES**, but you need to specify WMS.Web as startup project:

```powershell
# From WMS.Domain folder
cd WMS.Domain

dotnet ef migrations add YourMigration --startup-project ..\WMS.Web
dotnet ef database update --startup-project ..\WMS.Web

# Or from solution root (easier)
cd ..
.\migrate.ps1 -Action add -MigrationName "YourMigration"
.\migrate.ps1 -Action update
```

### Why Startup Project is Needed

- ? WMS.Domain has DbContext
- ? WMS.Domain has NO connection string
- ? WMS.Web has connection string
- ? WMS.Web provides config to EF Core

### Recommended Approach

**Use the helper script** (already configured for WMS.Web):

```powershell
.\migrate.ps1 -Action add -MigrationName "YourMigration"
.\migrate.ps1 -Action update
```

**You're all set! ??**
