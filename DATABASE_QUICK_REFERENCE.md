# ??? Database Quick Reference Card

## Your Current Configuration

```
Server:   CONGTAM-PC
Database: WMSDB
Auth:     Windows Authentication (Trusted_Connection=True)
Tables:   15 tables (all entities from WMS.Domain)
```

---

## ?? Create Database (One Command)

```powershell
# EASIEST WAY - Use helper script
.\migrate.ps1 -Action update

# OR manually
dotnet ef database update --project WMS.Domain --startup-project WMS.Auth.API
```

This creates:
- ? Database `WMSDB` on `CONGTAM-PC`
- ? All 15 tables with relationships
- ? Admin user (username: `admin`, password: `Admin@123`)
- ? 3 Roles (Admin, Manager, WarehouseStaff)

---

## ?? All Database Tables

| # | Table | Entities | Purpose |
|---|-------|----------|---------|
| 1 | Products | Product | Product catalog |
| 2 | Locations | Location | Warehouse locations |
| 3 | Inventories | Inventory | Current stock levels |
| 4 | InventoryTransactions | InventoryTransaction | Stock movements |
| 5 | Inbounds | Inbound | Receiving orders |
| 6 | InboundItems | InboundItem | Inbound line items |
| 7 | Outbounds | Outbound | Shipping orders |
| 8 | OutboundItems | OutboundItem | Outbound line items |
| 9 | Payments | Payment | Payment records |
| 10 | PaymentEvents | PaymentEvent | Payment history |
| 11 | Deliveries | Delivery | Delivery tracking |
| 12 | DeliveryEvents | DeliveryEvent | Delivery history |
| 13 | Users | User | System users |
| 14 | Roles | Role | User roles |
| 15 | UserRoles | UserRole | User-Role mapping |

---

## ?? Connection String

### Current (All microservices use this)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

### Update Connection String (All projects at once)

```powershell
# Use default (CONGTAM-PC\WMSDB)
.\update-connection-string.ps1

# Or specify custom server/database
.\update-connection-string.ps1 -Server "localhost" -Database "MyWMS"

# Or use custom connection string
.\update-connection-string.ps1 -CustomConnectionString "Server=...;Database=...;"
```

---

## ?? Database Relationships

```
Products
  ??? Inventories ??? Locations
  ??? InventoryTransactions
  ??? InboundItems ??? Inbounds
  ??? OutboundItems ??? Outbounds
                         ??? Payments ??? PaymentEvents
                         ??? Deliveries ??? DeliveryEvents

Users ??? UserRoles ??? Roles
```

---

## ? Verify Database

### Check if database exists
```powershell
# Using dotnet ef
dotnet ef dbcontext info --project WMS.Domain --startup-project WMS.Auth.API

# Using SSMS
# Connect to: CONGTAM-PC
# Look for: WMSDB database
```

### Check tables
```sql
-- Connect to WMSDB in SSMS
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### Test with API
```powershell
# Start Auth API
cd WMS.Auth.API
dotnet run

# Test login (in new terminal)
curl -X POST http://localhost:5001/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'

# Should return JWT token if database is working
```

---

## ?? Common Operations

### Drop and Recreate Database
```powershell
# Drop database
.\migrate.ps1 -Action drop

# Recreate
.\migrate.ps1 -Action update
```

### Add New Migration
```powershell
# After modifying entities in WMS.Domain/Entities/
.\migrate.ps1 -Action add -MigrationName "YourMigration"
.\migrate.ps1 -Action update
```

### Generate SQL Script
```powershell
.\migrate.ps1 -Action script
# Creates: migration_[timestamp].sql
```

### List All Migrations
```powershell
.\migrate.ps1 -Action list
```

---

## ?? Default Data (Seed Data)

After database creation, you'll have:

### Admin User
```
Username: admin
Password: Admin@123
Email:    admin@wms.com
```

### Roles
1. **Admin** - Full system access
2. **Manager** - Warehouse management
3. **WarehouseStaff** - Operations only

---

## ??? Troubleshooting

| Issue | Solution |
|-------|----------|
| "Cannot open database" | Run: `.\migrate.ps1 -Action update` |
| "Login failed" | Check SQL Server is running<br>Try: `Server=localhost` |
| "Network error" | Verify server name: `CONGTAM-PC`<br>Check SQL Server is accessible |
| Migration already applied | Normal! Database exists.<br>To add changes: `.\migrate.ps1 -Action add -MigrationName "NewMigration"` |

---

## ?? Files Location

| What | Where |
|------|-------|
| **Entities** | `WMS.Domain/Entities/*.cs` |
| **DbContext** | `WMS.Domain/Data/WMSDbContext.cs` |
| **Migrations** | `WMS.Domain/Migrations/*.cs` |
| **Connection Strings** | `*.API/appsettings.json` |
| **Helper Scripts** | `migrate.ps1`, `update-connection-string.ps1` |

---

## ?? Quick Start Workflow

### 1?? **Create Database**
```powershell
.\migrate.ps1 -Action update
```

### 2?? **Start All Services**
```powershell
.\run-all-services.ps1
```

### 3?? **Test APIs**
- Gateway: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### 4?? **Login to System**
- Use: `admin` / `Admin@123`
- Get JWT token
- Access protected endpoints

---

## ?? Documentation

| Document | Description |
|----------|-------------|
| `CREATE_DATABASE_GUIDE.md` | Complete database creation guide |
| `MIGRATION_QUICK_REFERENCE.md` | EF migrations commands |
| `DOMAIN_SETUP_COMPLETE.md` | Domain architecture overview |
| `SHARED_DATABASE_ARCHITECTURE.md` | Shared database pattern details |

---

## ?? Pro Tips

? **Always use migrations** - Don't create tables manually  
? **Backup before drop** - Use `.\migrate.ps1 -Action script` first  
? **Test locally first** - Verify on dev before production  
? **Keep migrations small** - One feature per migration  
? **Name migrations clearly** - Use descriptive names  

---

## ?? You're Ready!

Your database setup is complete when you can:

- ? Connect to `CONGTAM-PC\WMSDB`
- ? See all 15 tables in SSMS
- ? Login with `admin` / `Admin@123`
- ? Get JWT token from Auth API
- ? Start all microservices successfully

**Need help? See `CREATE_DATABASE_GUIDE.md` for detailed instructions.**
