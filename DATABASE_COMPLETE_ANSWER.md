# ?? WMS Database - Complete Answer

## ? Quick Answer

### How to Create Database?

**One Command:**
```powershell
.\migrate.ps1 -Action update
```

This creates the complete database with all entities!

---

## ?? Your Current Setup

### Connection String (All Microservices)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Database Details
| Setting | Value |
|---------|-------|
| **Server** | `CONGTAM-PC` (Your SQL Server instance) |
| **Database** | `WMSDB` |
| **Authentication** | Windows Authentication |
| **Tables** | 15 tables (all entities from `WMS.Domain/Entities/`) |
| **Migration Files** | Located in `WMS.Domain/Migrations/` |

---

## ??? Complete Entity List ? Database Tables

Your system has **15 entities** in `WMS.Domain/Entities/` that become **15 tables**:

### 1. Products Module (1 table)
```
WMS.Domain/Entities/Product.cs
  ?
Products table
  - Id (GUID, PK)
  - SKU (string, unique)
  - Name, Description, Category
  - UOM, Weight, Length, Width, Height
  - ReorderLevel, MaxStockLevel
  - Status (Active/Inactive)
  - Barcode
  - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
```

### 2. Locations Module (1 table)
```
WMS.Domain/Entities/Location.cs
  ?
Locations table
  - Id (GUID, PK)
  - Code (string, unique)
  - Name, Description
  - Zone, Aisle, Rack, Shelf, Bin
  - LocationType (enum)
  - Capacity, CurrentOccupancy
  - ParentLocationId (FK ? Locations)
  - IsActive
  - Audit fields
```

### 3. Inventory Module (2 tables)
```
WMS.Domain/Entities/Inventory.cs
  ?
Inventories table
  - Id (GUID, PK)
  - ProductId (FK ? Products)
  - LocationId (FK ? Locations)
  - QuantityOnHand, QuantityReserved
  - LastStockTakeDate
  - Audit fields

WMS.Domain/Entities/InventoryTransaction.cs
  ?
InventoryTransactions table
  - Id (GUID, PK)
  - TransactionNumber (string)
  - ProductId (FK ? Products)
  - TransactionType (enum)
  - Quantity, BalanceBefore, BalanceAfter
  - ReferenceType, ReferenceId
  - Notes
  - CreatedAt, CreatedBy
```

### 4. Inbound Module (2 tables)
```
WMS.Domain/Entities/Inbound.cs
  ?
Inbounds table
  - Id (GUID, PK)
  - InboundNumber (string, unique)
  - SupplierName
  - Status (Draft/Scheduled/InProgress/Completed/Cancelled)
  - ExpectedDate, ReceivedDate
  - Notes
  - Audit fields

WMS.Domain/Entities/InboundItem.cs
  ?
InboundItems table
  - Id (GUID, PK)
  - InboundId (FK ? Inbounds)
  - ProductId (FK ? Products)
  - LocationId (FK ? Locations)
  - ExpectedQuantity, ReceivedQuantity, DamagedQuantity
  - LotNumber, ExpiryDate
  - Notes
```

### 5. Outbound Module (2 tables)
```
WMS.Domain/Entities/Outbound.cs
  ?
Outbounds table
  - Id (GUID, PK)
  - OutboundNumber (string, unique)
  - CustomerName
  - Status (Draft/Scheduled/Picking/Picked/Shipped/Delivered/Cancelled)
  - OrderDate, ShipDate
  - ShippingAddress
  - PaymentId (FK ? Payments)
  - DeliveryId (FK ? Deliveries)
  - Notes
  - Audit fields

WMS.Domain/Entities/OutboundItem.cs
  ?
OutboundItems table
  - Id (GUID, PK)
  - OutboundId (FK ? Outbounds)
  - ProductId (FK ? Products)
  - LocationId (FK ? Locations)
  - OrderedQuantity, PickedQuantity, ShippedQuantity
  - Notes
```

### 6. Payment Module (2 tables)
```
WMS.Domain/Entities/Payment.cs
  ?
Payments table
  - Id (GUID, PK)
  - PaymentNumber (string, unique)
  - OutboundId (FK ? Outbounds)
  - Amount, Currency
  - PaymentMethod (Cash/CreditCard/BankTransfer/COD)
  - Status (Pending/Authorized/Captured/Failed/Refunded)
  - ExternalPaymentId
  - PaymentDate
  - Notes
  - Audit fields

WMS.Domain/Entities/PaymentEvent.cs
  ?
PaymentEvents table
  - Id (GUID, PK)
  - PaymentId (FK ? Payments)
  - EventType
  - EventData
  - CreatedAt
```

### 7. Delivery Module (2 tables)
```
WMS.Domain/Entities/Delivery.cs
  ?
Deliveries table
  - Id (GUID, PK)
  - DeliveryNumber (string, unique)
  - OutboundId (FK ? Outbounds)
  - Status (Pending/PickedUp/InTransit/OutForDelivery/Delivered/Failed/Returned)
  - ShippingAddress, City, State, ZipCode, Country
  - Carrier, TrackingNumber
  - VehicleNumber, DriverName, DriverPhone
  - PickupDate, EstimatedDeliveryDate, ActualDeliveryDate
  - DeliveryNotes, FailureReason
  - IsReturn, ReturnInboundId
  - Audit fields

WMS.Domain/Entities/DeliveryEvent.cs
  ?
DeliveryEvents table
  - Id (GUID, PK)
  - DeliveryId (FK ? Deliveries)
  - EventType
  - Location
  - EventDate
  - Notes
```

### 8. Authentication Module (3 tables)
```
WMS.Domain/Entities/User.cs
  ?
Users table
  - Id (GUID, PK)
  - Username (string, unique)
  - Email (string, unique)
  - PasswordHash
  - FirstName, LastName
  - IsActive
  - LastLoginAt
  - RefreshToken, RefreshTokenExpiry
  - Audit fields

WMS.Domain/Entities/Role.cs
  ?
Roles table
  - Id (GUID, PK)
  - Name (string, unique)
  - Description
  - Audit fields

WMS.Domain/Entities/UserRole.cs
  ?
UserRoles table (Composite PK)
  - UserId (PK, FK ? Users)
  - RoleId (PK, FK ? Roles)
  - AssignedAt
  - AssignedBy
```

---

## ?? Step-by-Step: Create Database

### Method 1: Using Helper Script (RECOMMENDED ?)

```powershell
# 1. Navigate to solution root
cd F:\PROJECT\STUDY\VMS

# 2. Run migration
.\migrate.ps1 -Action update

# Done! Database created with all 15 tables
```

### Method 2: Using Manual Command

```powershell
# From solution root
dotnet ef database update `
    --project WMS.Domain `
    --startup-project WMS.Auth.API `
    --context WMSDbContext
```

### What Gets Created?

? **Database**: `WMSDB` on server `CONGTAM-PC`  
? **15 Tables**: All entities from `WMS.Domain/Entities/`  
? **Relationships**: All foreign keys configured  
? **Indexes**: Unique indexes on SKU, Username, Email, etc.  
? **Seed Data**:
   - 3 Roles: Admin, Manager, WarehouseStaff
   - 1 Admin User: username `admin`, password `Admin@123`

---

## ?? Connection String Details

### Where Are Connection Strings?

All microservices have their own `appsettings.json`:

```
WMS.Auth.API/appsettings.json
WMS.Products.API/appsettings.json
WMS.Locations.API/appsettings.json
WMS.Inventory.API/appsettings.json
WMS.Inbound.API/appsettings.json
WMS.Outbound.API/appsettings.json
WMS.Payment.API/appsettings.json
WMS.Delivery.API/appsettings.json
```

### Current Connection String (All Use Same)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Connection String Breakdown

```
Server=CONGTAM-PC              ? Your SQL Server instance name
Database=WMSDB                 ? Database name
Trusted_Connection=True        ? Use Windows Authentication
MultipleActiveResultSets=true  ? Allow multiple active queries
TrustServerCertificate=True    ? Trust SSL certificate
Encrypt=False                  ? No encryption (local dev)
```

### Change Connection String (All Projects)

```powershell
# Update all at once
.\update-connection-string.ps1 -Server "localhost" -Database "WMSDB"

# Or with custom connection string
.\update-connection-string.ps1 -CustomConnectionString "Server=...;Database=...;"
```

---

## ?? Database Schema Summary

### Table Count: 15 Tables

| Module | Tables | Total |
|--------|--------|-------|
| Products | Products | 1 |
| Locations | Locations | 1 |
| Inventory | Inventories, InventoryTransactions | 2 |
| Inbound | Inbounds, InboundItems | 2 |
| Outbound | Outbounds, OutboundItems | 2 |
| Payment | Payments, PaymentEvents | 2 |
| Delivery | Deliveries, DeliveryEvents | 2 |
| Auth | Users, Roles, UserRoles | 3 |
| **TOTAL** | | **15** |

### Entity Relationships

```
Products
  ??? Inventories (FK: ProductId)
  ?     ??? Locations (FK: LocationId)
  ??? InventoryTransactions (FK: ProductId)
  ??? InboundItems (FK: ProductId)
  ?     ??? Inbounds (FK: InboundId)
  ??? OutboundItems (FK: ProductId)
        ??? Outbounds (FK: OutboundId)
              ??? Payments (FK: PaymentId)
              ?     ??? PaymentEvents (FK: PaymentId)
              ??? Deliveries (FK: DeliveryId)
                    ??? DeliveryEvents (FK: DeliveryId)

Users
  ??? UserRoles (FK: UserId)
        ??? Roles (FK: RoleId)
```

---

## ? Verify Database Creation

### 1. Check with SQL Server Management Studio (SSMS)

```
1. Open SSMS
2. Connect to: CONGTAM-PC
3. Expand: Databases ? WMSDB ? Tables
4. Should see 15 tables:
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
```

### 2. Check with PowerShell

```powershell
# List all migrations
.\migrate.ps1 -Action list

# Show database info
dotnet ef dbcontext info `
    --project WMS.Domain `
    --startup-project WMS.Auth.API
```

### 3. Test with API

```powershell
# Start Auth API
cd WMS.Auth.API
dotnet run

# In new terminal, test login
curl -X POST http://localhost:5001/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'

# Should return JWT token if database works!
```

---

## ?? Common Tasks

### Create Database (First Time)
```powershell
.\migrate.ps1 -Action update
```

### Reset Database (Drop and Recreate)
```powershell
.\migrate.ps1 -Action drop
.\migrate.ps1 -Action update
```

### Add New Entity/Column
```powershell
# 1. Modify entity in WMS.Domain/Entities/
# 2. Create migration
.\migrate.ps1 -Action add -MigrationName "AddNewField"
# 3. Apply to database
.\migrate.ps1 -Action update
```

### Generate SQL Script
```powershell
.\migrate.ps1 -Action script
# Creates: migration_[timestamp].sql
```

---

## ??? Troubleshooting

### "Cannot open database WMSDB"
**Solution**: Database doesn't exist yet
```powershell
.\migrate.ps1 -Action update
```

### "Login failed for user"
**Solutions**:
1. Check SQL Server is running
2. Verify server name: `CONGTAM-PC`
3. Try `Server=localhost` or `Server=.`

### "A network-related error occurred"
**Solutions**:
1. Check SQL Server service is running
2. Verify firewall allows SQL Server
3. Check server name spelling

### Migration already applied
**This is normal!** Database exists.  
To add new changes:
```powershell
.\migrate.ps1 -Action add -MigrationName "YourNewMigration"
```

---

## ?? Related Documentation

| Document | Purpose |
|----------|---------|
| `CREATE_DATABASE_GUIDE.md` | Complete database creation guide |
| `DATABASE_QUICK_REFERENCE.md` | Quick reference card |
| `MIGRATION_QUICK_REFERENCE.md` | Migration commands |
| `DOMAIN_SETUP_COMPLETE.md` | Domain architecture |
| `SHARED_DATABASE_ARCHITECTURE.md` | Shared DB pattern |

---

## ?? Summary

### Your Database Setup

**Connection String**:
```
Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False
```

**Entities ? Tables**:
- ? 15 entities in `WMS.Domain/Entities/`
- ? Create 15 database tables
- ? All relationships configured
- ? Seed data included

**Create Database**:
```powershell
.\migrate.ps1 -Action update
```

**Default Admin**:
- Username: `admin`
- Password: `Admin@123`

**You're ready to go! ??**
