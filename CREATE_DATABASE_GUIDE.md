# ??? WMS Database Creation Guide

## ?? Overview

Your WMS system uses **Entity Framework Core Code-First** approach with a **shared database** for all microservices.

### Current Database Configuration

| Configuration | Value |
|--------------|-------|
| **Database Name** | `WMSDB` |
| **Server** | `CONGTAM-PC` |
| **Provider** | SQL Server |
| **Migration Assembly** | `WMS.Domain` |
| **All Entities** | Defined in `WMS.Domain/Entities/` |

---

## ?? Your Current Connection String

All microservices use this connection string (from `appsettings.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Connection String Breakdown

| Component | Value | Purpose |
|-----------|-------|---------|
| **Server** | `CONGTAM-PC` | Your SQL Server instance name |
| **Database** | `WMSDB` | Database name |
| **Trusted_Connection** | `True` | Use Windows Authentication |
| **MultipleActiveResultSets** | `true` | Allow multiple queries simultaneously |
| **TrustServerCertificate** | `True` | Trust server SSL certificate |
| **Encrypt** | `False` | No SSL encryption (local dev) |

---

## ?? Database Schema - All Entities

Your database will contain **15 tables** based on these entities:

### 1. **Products Module**
```
Products
??? Id (GUID, PK)
??? SKU (string, unique)
??? Name (string)
??? Description (string)
??? Category (string)
??? UOM (string)
??? Weight, Length, Width, Height (decimal)
??? ReorderLevel, MaxStockLevel (decimal)
??? Status (enum: Active/Inactive)
??? Barcode (string)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
```

### 2. **Locations Module**
```
Locations
??? Id (GUID, PK)
??? Code (string, unique)
??? Name (string)
??? Description (string)
??? Zone, Aisle, Rack, Shelf, Bin (string)
??? LocationType (enum: Warehouse/Zone/Aisle/Rack/Shelf/Bin)
??? Capacity, CurrentOccupancy (decimal)
??? ParentLocationId (GUID, FK ? Locations)
??? IsActive (bool)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
```

### 3. **Inventory Module**
```
Inventories
??? Id (GUID, PK)
??? ProductId (GUID, FK ? Products)
??? LocationId (GUID, FK ? Locations)
??? QuantityOnHand (decimal)
??? QuantityReserved (decimal)
??? LastStockTakeDate (datetime)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

InventoryTransactions
??? Id (GUID, PK)
??? TransactionNumber (string)
??? ProductId (GUID, FK ? Products)
??? TransactionType (enum: Adjustment/Transfer/Receipt/Issue)
??? Quantity (decimal)
??? BalanceBefore, BalanceAfter (decimal)
??? ReferenceType, ReferenceId (string, GUID)
??? Notes (string)
??? CreatedAt, CreatedBy
```

### 4. **Inbound Module**
```
Inbounds
??? Id (GUID, PK)
??? InboundNumber (string, unique)
??? SupplierName (string)
??? Status (enum: Draft/Scheduled/InProgress/Completed/Cancelled)
??? ExpectedDate, ReceivedDate (datetime)
??? Notes (string)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

InboundItems
??? Id (GUID, PK)
??? InboundId (GUID, FK ? Inbounds)
??? ProductId (GUID, FK ? Products)
??? LocationId (GUID, FK ? Locations)
??? ExpectedQuantity (decimal)
??? ReceivedQuantity (decimal)
??? DamagedQuantity (decimal)
??? LotNumber, ExpiryDate (string, datetime)
??? Notes (string)
```

### 5. **Outbound Module**
```
Outbounds
??? Id (GUID, PK)
??? OutboundNumber (string, unique)
??? CustomerName (string)
??? Status (enum: Draft/Scheduled/Picking/Picked/Shipped/Delivered/Cancelled)
??? OrderDate, ShipDate (datetime)
??? ShippingAddress (string)
??? PaymentId (GUID, FK ? Payments)
??? DeliveryId (GUID, FK ? Deliveries)
??? Notes (string)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

OutboundItems
??? Id (GUID, PK)
??? OutboundId (GUID, FK ? Outbounds)
??? ProductId (GUID, FK ? Products)
??? LocationId (GUID, FK ? Locations)
??? OrderedQuantity (decimal)
??? PickedQuantity (decimal)
??? ShippedQuantity (decimal)
??? Notes (string)
```

### 6. **Payment Module**
```
Payments
??? Id (GUID, PK)
??? PaymentNumber (string, unique)
??? OutboundId (GUID, FK ? Outbounds)
??? Amount (decimal)
??? Currency (string)
??? PaymentMethod (enum: Cash/CreditCard/BankTransfer/COD)
??? Status (enum: Pending/Authorized/Captured/Failed/Refunded)
??? ExternalPaymentId (string)
??? PaymentDate (datetime)
??? Notes (string)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

PaymentEvents
??? Id (GUID, PK)
??? PaymentId (GUID, FK ? Payments)
??? EventType (string)
??? EventData (string)
??? CreatedAt (datetime)
```

### 7. **Delivery Module**
```
Deliveries
??? Id (GUID, PK)
??? DeliveryNumber (string, unique)
??? OutboundId (GUID, FK ? Outbounds)
??? Status (enum: Pending/PickedUp/InTransit/OutForDelivery/Delivered/Failed/Returned)
??? ShippingAddress, City, State, ZipCode, Country (string)
??? Carrier, TrackingNumber (string)
??? VehicleNumber, DriverName, DriverPhone (string)
??? PickupDate, EstimatedDeliveryDate, ActualDeliveryDate (datetime)
??? DeliveryNotes, FailureReason (string)
??? IsReturn (bool)
??? ReturnInboundId (GUID)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

DeliveryEvents
??? Id (GUID, PK)
??? DeliveryId (GUID, FK ? Deliveries)
??? EventType (string)
??? Location (string)
??? EventDate (datetime)
??? Notes (string)
```

### 8. **Authentication Module**
```
Users
??? Id (GUID, PK)
??? Username (string, unique)
??? Email (string, unique)
??? PasswordHash (string)
??? FirstName, LastName (string)
??? IsActive (bool)
??? LastLoginAt (datetime)
??? RefreshToken, RefreshTokenExpiry (string, datetime)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

Roles
??? Id (GUID, PK)
??? Name (string, unique)
??? Description (string)
??? CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

UserRoles
??? UserId (GUID, PK, FK ? Users)
??? RoleId (GUID, PK, FK ? Roles)
??? AssignedAt (datetime)
??? AssignedBy (string)
```

---

## ?? Method 1: Create Database Using EF Migrations (RECOMMENDED)

This is the **easiest and safest** method because:
- ? Migration files already exist in `WMS.Domain/Migrations/`
- ? All entity configurations are defined
- ? Seed data is included (admin user, roles)
- ? One command creates everything

### Step 1: Verify SQL Server is Running

```powershell
# Check if SQL Server is running
Get-Service -Name "MSSQL*" | Select-Object Name, Status, DisplayName

# Or check SQL Server Management Studio (SSMS)
# Connect to: CONGTAM-PC
```

### Step 2: Run Migration Using Helper Script

```powershell
# From solution root directory
.\migrate.ps1 -Action update

# Or specify a startup project
.\migrate.ps1 -Action update -StartupProject Inbound
```

### Step 3: OR Run Migration Manually

```powershell
# From solution root
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Auth.API `
    --context WMSDbContext
```

### What This Creates

? Database: `WMSDB` on server `CONGTAM-PC`  
? All 15 tables with proper relationships  
? Primary keys, foreign keys, indexes  
? Unique constraints (SKU, Username, Email, etc.)  
? Seed data:
- 3 Roles: Admin, Manager, WarehouseStaff
- 1 Admin user (username: `admin`, password: `Admin@123`)

---

## ?? Method 2: Create Database Using SQL Script

If you prefer SQL scripts or need to deploy to production:

### Step 1: Generate SQL Script

```powershell
# Generate SQL script from migrations
.\migrate.ps1 -Action script

# Or manually
dotnet ef migrations script `
    --project WMS.Domain `
    --startup-project WMS.Auth.API `
    --output create-database.sql `
    --idempotent
```

This creates `create-database.sql` file.

### Step 2: Execute SQL Script

**Option A: Using SSMS**
1. Open SQL Server Management Studio
2. Connect to `CONGTAM-PC`
3. File ? Open ? `create-database.sql`
4. Execute (F5)

**Option B: Using sqlcmd**
```powershell
sqlcmd -S CONGTAM-PC -i create-database.sql
```

---

## ??? Method 3: Manual Database Creation (NOT RECOMMENDED)

Only use this if you can't use migrations.

### Step 1: Create Database

```sql
-- Connect to SQL Server (CONGTAM-PC)
CREATE DATABASE WMSDB;
GO

USE WMSDB;
GO
```

### Step 2: Create All Tables

See `create-tables.sql` file (if it exists) or use the migration script.

**?? Warning**: Manual creation is error-prone and doesn't include:
- Proper indexes
- Seed data
- Migration history (you can't use `dotnet ef database update` later)

---

## ? Verify Database Creation

### Method 1: Using SSMS

1. Open SQL Server Management Studio
2. Connect to `CONGTAM-PC`
3. Expand Databases ? `WMSDB` ? Tables
4. You should see 15 tables:
   - Products
   - Locations
   - Inventories
   - InventoryTransactions
   - Inbounds
   - InboundItems
   - Outbounds
   - OutboundItems
   - Payments
   - PaymentEvents
   - Deliveries
   - DeliveryEvents
   - Users
   - Roles
   - UserRoles

### Method 2: Using PowerShell

```powershell
# Check if database exists
dotnet ef dbcontext info `
    --project WMS.Domain `
    --startup-project WMS.Auth.API
```

### Method 3: Test with API

```powershell
# Start Auth API
cd WMS.Auth.API
dotnet run

# Test login (should return token)
curl -X POST http://localhost:5001/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? Connection String Options

### Current (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### SQL Authentication (Instead of Windows Auth)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### LocalDB (For Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Production (Azure SQL)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Database=WMSDB;User Id=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"
  }
}
```

---

## ?? Updating Connection String

All microservices have their own `appsettings.json`. To change the connection string:

### Option 1: Update Each File Manually

Files to update:
- `WMS.Auth.API/appsettings.json`
- `WMS.Products.API/appsettings.json`
- `WMS.Locations.API/appsettings.json`
- `WMS.Inventory.API/appsettings.json`
- `WMS.Inbound.API/appsettings.json`
- `WMS.Outbound.API/appsettings.json`
- `WMS.Payment.API/appsettings.json`
- `WMS.Delivery.API/appsettings.json`

### Option 2: Use PowerShell Script

```powershell
# Create update-connection-string.ps1
param([string]$NewConnectionString)

$files = Get-ChildItem -Path "*.API" -Recurse -Filter "appsettings.json"

foreach ($file in $files) {
    $json = Get-Content $file.FullName | ConvertFrom-Json
    $json.ConnectionStrings.DefaultConnection = $NewConnectionString
    $json | ConvertTo-Json -Depth 10 | Set-Content $file.FullName
}

Write-Host "Connection string updated in $($files.Count) files"
```

Usage:
```powershell
.\update-connection-string.ps1 -NewConnectionString "Server=NewServer;Database=WMSDB;..."
```

---

## ?? Quick Start Commands

### Create Database (Fastest)

```powershell
# Option 1: Using helper script
.\migrate.ps1 -Action update

# Option 2: Manual command
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API
```

### Drop and Recreate Database

```powershell
# Drop database
.\migrate.ps1 -Action drop

# Recreate database
.\migrate.ps1 -Action update
```

### Check Migration Status

```powershell
# List all migrations
.\migrate.ps1 -Action list

# Show database info
dotnet ef dbcontext info --project WMS.Domain --startup-project WMS.Auth.API
```

---

## ?? Database Relationships

```
Users ??? UserRoles ??? Roles
       ?
Products ??? Inventories ??? Locations
          ?
          ?? InventoryTransactions
          ?
          ?? InboundItems ??? Inbounds
          ?
          ?? OutboundItems ??? Outbounds ??? Payments ??? PaymentEvents
                                           ?
                                           ?? Deliveries ??? DeliveryEvents
```

---

## ??? Troubleshooting

### Issue 1: "Cannot open database WMSDB"

**Cause**: Database doesn't exist yet  
**Solution**: Run migration to create database
```powershell
.\migrate.ps1 -Action update
```

### Issue 2: "Login failed for user"

**Cause**: SQL Server authentication issue  
**Solutions**:
1. Check if SQL Server is running
2. Verify Windows Authentication is enabled
3. Check SQL Server allows remote connections
4. Try SQL Authentication instead

### Issue 3: "A network-related error occurred"

**Cause**: Can't connect to SQL Server  
**Solutions**:
1. Verify server name: `CONGTAM-PC`
2. Check SQL Server is running
3. Check firewall allows SQL Server
4. Try: `Server=localhost` or `Server=.`

### Issue 4: Migration already applied

**Error**: "The migration 'InitialCreate' has already been applied"  
**Solution**: This is normal. Database already exists. To add new changes:
```powershell
# Make entity changes, then:
.\migrate.ps1 -Action add -MigrationName "YourNewMigration"
.\migrate.ps1 -Action update
```

---

## ?? Seed Data

After database creation, you'll have:

### Default Admin User
```
Username: admin
Password: Admin@123
Email: admin@wms.com
```

### Default Roles
1. **Admin** - System Administrator
2. **Manager** - Warehouse Manager  
3. **WarehouseStaff** - Warehouse Staff

### Test Login

```powershell
# Start Auth API
cd WMS.Auth.API
dotnet run

# Test in new terminal
curl -X POST http://localhost:5001/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? Summary

### To Create Your Database

**Recommended: Use EF Migrations**
```powershell
# One command to create everything!
.\migrate.ps1 -Action update
```

**This Creates**:
- ? Database: `WMSDB` on `CONGTAM-PC`
- ? 15 tables with relationships
- ? Indexes and constraints
- ? Seed data (admin user + roles)
- ? Ready to use immediately!

### Your Connection String
```
Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False
```

### Next Steps After Database Creation
1. ? Database created
2. ? Start microservices: `.\run-all-services.ps1`
3. ? Test APIs: http://localhost:5000 (Gateway)
4. ? Start building features!

**Your database is ready! ??**
