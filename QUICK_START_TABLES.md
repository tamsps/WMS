# ? QUICK START - Create Database Tables

## You Are Here:
- ? Empty WMSDB database exists on CONGTAM-PC
- ? Connection strings configured
- ? Migrations ready
- ? Need to create tables

---

## ?? DO THIS NOW:

### Step 1: Open Visual Studio Package Manager Console
**Menu**: `Tools` ? `NuGet Package Manager` ? `Package Manager Console`

### Step 2: Set Default Project
**Dropdown**: Select `WMS.Infrastructure`

### Step 3: Run This Command
```powershell
Update-Database
```

### Step 4: Wait for "Done"
You'll see migration output, wait for:
```
Done.
```

---

## ? That's It!

Your database now has:
- 16 tables with all relationships
- 3 roles (Admin, Manager, WarehouseStaff)
- 1 admin user (admin / Admin@123)
- Ready for all microservices

---

## ?? Next Steps

### Start All Services:
```powershell
.\run-all-services.ps1
```

### Access Applications:
- **Gateway**: https://localhost:7000
- **Auth API**: https://localhost:7001
- **Web App**: http://localhost:5000

### Login:
- **Username**: admin
- **Password**: Admin@123

---

## ?? Need More Details?

See: **CREATE_TABLES_GUIDE.md** (3 different methods explained)

---

## ? Having Issues?

### Can't find Package Manager Console?
Try: `View` ? `Other Windows` ? `Package Manager Console`

### Migration fails?
1. Check SQL Server is running
2. Verify WMSDB exists
3. See troubleshooting in CREATE_TABLES_GUIDE.md

### Want to verify tables?
Run in SSMS:
```sql
USE WMSDB;
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

---

**Time to complete**: ~30 seconds  
**Complexity**: ? (Very Easy)
