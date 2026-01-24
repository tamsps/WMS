# Database Table Creation Guide

## Quick Start - Create Tables in WMSDB

You have already created the empty WMSDB database. Now follow ONE of these methods to create the tables:

---

## ? METHOD 1: Visual Studio Package Manager Console (EASIEST)

1. **Open Visual Studio**
2. **Open Package Manager Console**
   - Go to: `Tools` ? `NuGet Package Manager` ? `Package Manager Console`

3. **Set the Default Project**
   - In the Package Manager Console, use the dropdown to select: `WMS.Infrastructure`

4. **Run the Migration Command**
   ```powershell
   Update-Database
   ```

5. **Wait for Completion**
   - You should see output showing the migration being applied
   - Look for "Done" at the end

---

## ? METHOD 2: Visual Studio Developer PowerShell

1. **Open Developer PowerShell**
   - In Visual Studio: `View` ? `Terminal`
   - Or use `View` ? `Other Windows` ? `Package Manager Console`

2. **Navigate to Solution Directory**
   ```powershell
   cd F:\PROJECT\STUDY\VMS
   ```

3. **Run Migration**
   ```powershell
   dotnet ef database update --project .\WMS.Infrastructure\ --startup-project .\WMS.Auth.API\
   ```

---

## ? METHOD 3: Regular PowerShell/Command Prompt

1. **Open PowerShell or Command Prompt**

2. **Install EF Core Tools** (if not already installed)
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

3. **Navigate to Solution Directory**
   ```powershell
   cd F:\PROJECT\STUDY\VMS
   ```

4. **Build the Solution**
   ```powershell
   dotnet build
   ```

5. **Apply Migrations**
   ```powershell
   dotnet ef database update --project WMS.Infrastructure --startup-project WMS.Auth.API
   ```

---

## What Tables Will Be Created?

The migration will create the following tables in WMSDB:

### Authentication & Security
- **Users** - System users with encrypted passwords
- **Roles** - User roles (Admin, Manager, WarehouseStaff)
- **UserRoles** - User-to-Role assignments

### Master Data
- **Products** - Product catalog with SKUs
- **Locations** - Warehouse locations (zones, aisles, racks, etc.)

### Inventory Management
- **Inventories** - Current stock levels by product and location
- **InventoryTransactions** - Complete audit trail of stock movements

### Operations
- **Inbounds** - Receiving operations
- **InboundItems** - Received items details
- **Outbounds** - Shipping/fulfillment operations
- **OutboundItems** - Shipped items details

### Supporting Services
- **Payments** - Payment records
- **PaymentEvents** - Payment event history
- **Deliveries** - Delivery/shipment tracking
- **DeliveryEvents** - Delivery event history

---

## Default Seed Data

The migration automatically creates:

### 3 Roles:
1. **Admin** - System Administrator
2. **Manager** - Warehouse Manager  
3. **WarehouseStaff** - Warehouse Staff

### 1 Admin User:
- **Username**: `admin`
- **Password**: `Admin@123`
- **Email**: `admin@wms.com`
- **Role**: Admin

---

## Verification

### Check if Tables Were Created:

**Option 1: SQL Server Management Studio (SSMS)**
1. Connect to `CONGTAM-PC`
2. Expand `Databases` ? `WMSDB` ? `Tables`
3. You should see 17 tables (including `__EFMigrationsHistory`)

**Option 2: Using SQL Query**
```sql
USE WMSDB;
GO

SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected Output:**
```
__EFMigrationsHistory
DeliveryEvents
Deliveries
InboundItems
Inbounds
Inventories
InventoryTransactions
Locations
OutboundItems
Outbounds
PaymentEvents
Payments
Products
Roles
UserRoles
Users
```

---

## Troubleshooting

### Error: "Unable to create DbContext"
**Solution**: Make sure you're in the correct directory (F:\PROJECT\STUDY\VMS)

### Error: "Cannot connect to database"
**Solution**: 
1. Verify SQL Server is running on CONGTAM-PC
2. Check connection string in `WMS.Auth.API\appsettings.json`
3. Try connecting via SSMS first to confirm connectivity

### Error: "Project not found"
**Solution**: Use full paths:
```powershell
dotnet ef database update --project .\WMS.Infrastructure\WMS.Infrastructure.csproj --startup-project .\WMS.Auth.API\WMS.Auth.API.csproj
```

### Error: "dotnet-ef not found"
**Solution**: Install EF Core tools:
```powershell
dotnet tool install --global dotnet-ef
# Or update if already installed:
dotnet tool update --global dotnet-ef
```

---

## After Tables Are Created

1. **Verify the admin user**:
   ```sql
   USE WMSDB;
   SELECT * FROM Users WHERE Username = 'admin';
   SELECT * FROM Roles;
   SELECT * FROM UserRoles;
   ```

2. **Start the microservices**:
   ```powershell
   .\run-all-services.ps1
   ```

3. **Test the Auth API**:
   - Open browser to: `https://localhost:7001`
   - Login with: `admin` / `Admin@123`

4. **Access other services**:
   - Gateway: `https://localhost:7000`
   - Products API: `https://localhost:7002`
   - Locations API: `https://localhost:7003`
   - Inventory API: `https://localhost:7004`
   - Inbound API: `https://localhost:7005`
   - Outbound API: `https://localhost:7006`
   - Payment API: `https://localhost:7007`
   - Delivery API: `https://localhost:7008`

---

## Need Help?

If you encounter any issues:
1. Check the error message carefully
2. Verify SQL Server is running
3. Ensure WMSDB database exists (even if empty)
4. Try the migration command from Visual Studio Package Manager Console
5. Check connection string in appsettings.json files

**Connection String** (should be in all appsettings.json files):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
}
```

---

## Summary

**Recommended Approach:**
Use Visual Studio Package Manager Console (METHOD 1) as it's the most reliable and provides clear output.

**Command to run:**
```powershell
Update-Database
```

That's it! The tables will be created automatically with all relationships, indexes, and seed data.
