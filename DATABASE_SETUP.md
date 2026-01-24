# WMS Database Setup Guide

## Overview
This guide explains how to set up the WMSDB database on SQL Server (CONGTAM-PC) for the Warehouse Management System.

## Database Information
- **Server Name**: CONGTAM-PC
- **Database Name**: WMSDB
- **Authentication**: Windows Authentication (Trusted Connection)
- **Connection String**: `Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False`

## Prerequisites
1. SQL Server installed on CONGTAM-PC
2. SQL Server service running
3. Windows user has permissions to create databases
4. .NET 9 SDK installed
5. Entity Framework Core tools installed

## Database Entities

### Core Entities (WMS.Domain)

#### 1. **Authentication & Authorization**
- **Users** - System users with authentication credentials
  - Username (unique)
  - Email (unique)
  - PasswordHash
  - FirstName, LastName
  - IsActive status
  - RefreshToken support

- **Roles** - User roles (Admin, Manager, WarehouseStaff)
  - Name (unique)
  - Description

- **UserRoles** - Many-to-many relationship between Users and Roles

#### 2. **Product Management**
- **Products** - Master product data (SKU catalog)
  - SKU (unique identifier)
  - Name, Description
  - Status (Active/Inactive/Discontinued)
  - UOM (Unit of Measure)
  - Physical dimensions (Weight, Length, Width, Height)
  - Barcode, Category
  - Reorder levels

#### 3. **Location Management**
- **Locations** - Warehouse location hierarchy
  - Code (unique identifier)
  - Name, Description
  - Zone, Aisle, Rack, Shelf, Bin
  - Capacity tracking
  - Hierarchical structure (Parent-Child)
  - Location types

#### 4. **Inventory Management**
- **Inventories** - Current stock levels by product and location
  - ProductId + LocationId (unique combination)
  - QuantityOnHand
  - QuantityReserved
  - QuantityAvailable (calculated)
  - LastStockDate

- **InventoryTransactions** - Audit trail of all stock movements
  - TransactionNumber
  - TransactionType (Receive, Pick, Adjust, etc.)
  - Product, Location
  - Quantity changes
  - Balance tracking (before/after)
  - Reference to source document

#### 5. **Inbound Operations**
- **Inbounds** - Receiving operations
  - InboundNumber (unique)
  - ReferenceNumber (PO, Transfer Order)
  - Status (Pending, Receiving, Received, Cancelled)
  - ExpectedDate, ReceivedDate
  - Supplier information

- **InboundItems** - Line items for inbound receipts
  - Product, Location
  - ExpectedQuantity, ReceivedQuantity, DamagedQuantity
  - Lot tracking, Expiry dates

#### 6. **Outbound Operations**
- **Outbounds** - Shipping/fulfillment operations
  - OutboundNumber (unique)
  - OrderNumber
  - Status (Pending, Picking, Picked, Shipped, Delivered, Cancelled)
  - Customer information
  - Shipping address
  - Links to Payment and Delivery

- **OutboundItems** - Line items for outbound shipments
  - Product, Location
  - OrderedQuantity, PickedQuantity, ShippedQuantity
  - Serial/Lot number tracking

#### 7. **Payment Management**
- **Payments** - Payment records for outbound orders
  - PaymentNumber (unique)
  - OutboundId (optional link)
  - PaymentType, Status
  - Amount, Currency
  - External payment gateway integration
  - Transaction references

- **PaymentEvents** - Payment event history
  - Event tracking for payment lifecycle

#### 8. **Delivery Management**
- **Deliveries** - Delivery/shipment tracking
  - DeliveryNumber (unique)
  - OutboundId link
  - Status (Pending, InTransit, Delivered, Failed, Returned)
  - Shipping address details
  - Carrier, tracking number
  - Vehicle, driver information
  - Estimated and actual delivery dates

- **DeliveryEvents** - Delivery event tracking
  - Event history for delivery lifecycle
  - Location updates
  - Status changes

## Database Relationships

### Key Relationships:
1. **User ? UserRoles ? Role** (Many-to-Many)
2. **Product ? Inventories ? Location** (Stock by product and location)
3. **Product ? InventoryTransactions** (Transaction history)
4. **Inbound ? InboundItems ? Product/Location**
5. **Outbound ? OutboundItems ? Product/Location**
6. **Outbound ? Payment** (1-to-1, optional)
7. **Outbound ? Delivery** (1-to-1, optional)
8. **Payment ? PaymentEvents** (1-to-Many)
9. **Delivery ? DeliveryEvents** (1-to-Many)
10. **Location ? ChildLocations** (Hierarchical)

## Setup Instructions

### Method 1: Using PowerShell Script (Recommended)

1. Open PowerShell as Administrator
2. Navigate to the project root directory
3. Run the setup script:
   ```powershell
   .\setup-database.ps1
   ```

The script will:
- Check and install EF Core tools if needed
- Create or update migrations
- Apply migrations to create the database
- Seed initial data (admin user and roles)

### Method 2: Manual Setup

1. **Install EF Core Tools**:
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

2. **Navigate to Infrastructure Project**:
   ```powershell
   cd WMS.Infrastructure
   ```

3. **Create Migration** (if not exists):
   ```powershell
   dotnet ef migrations add InitialCreate --startup-project ..\WMS.Auth.API
   ```

4. **Update Database**:
   ```powershell
   dotnet ef database update --startup-project ..\WMS.Auth.API
   ```

### Method 3: Using Package Manager Console (Visual Studio)

1. Open Package Manager Console in Visual Studio
2. Set Default Project to `WMS.Infrastructure`
3. Run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

## Seed Data

The database is automatically seeded with:

### Roles:
- **Admin** - System Administrator
- **Manager** - Warehouse Manager
- **WarehouseStaff** - Warehouse Staff

### Default Admin User:
- **Username**: admin
- **Password**: Admin@123
- **Email**: admin@wms.com
- **Role**: Admin

## Microservices Database Access

All microservices share the same database (WMSDB) with the following configuration:

### Services Using Database:
1. **WMS.Auth.API** - User authentication and authorization
2. **WMS.Products.API** - Product master data
3. **WMS.Locations.API** - Location management
4. **WMS.Inventory.API** - Inventory levels and transactions
5. **WMS.Inbound.API** - Receiving operations
6. **WMS.Outbound.API** - Shipping operations
7. **WMS.Payment.API** - Payment processing
8. **WMS.Delivery.API** - Delivery tracking

Each service has its own `appsettings.json` configured with the same connection string.

## Connection String Configuration

All microservices use the following connection string in their `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  }
}
```

## Troubleshooting

### Cannot connect to SQL Server
1. Verify SQL Server is running:
   - Open Services (services.msc)
   - Check "SQL Server (MSSQLSERVER)" is running

2. Test connection using SQL Server Management Studio (SSMS)
   - Server name: CONGTAM-PC
   - Authentication: Windows Authentication

3. Check Windows Firewall settings
   - Ensure SQL Server port (default 1433) is not blocked

### Permission Denied
1. Ensure your Windows user has `db_creator` role
2. Right-click SQL Server in SSMS ? Properties ? Security
3. Ensure Windows Authentication is enabled

### Migration Errors
1. Clean the solution:
   ```powershell
   dotnet clean
   ```

2. Rebuild:
   ```powershell
   dotnet build
   ```

3. Remove existing migrations and recreate:
   ```powershell
   Remove-Item .\WMS.Infrastructure\Migrations -Recurse -Force
   dotnet ef migrations add InitialCreate --project WMS.Infrastructure --startup-project WMS.Auth.API
   ```

## Database Management

### View Database Schema
```sql
-- Connect to WMSDB using SSMS
USE WMSDB;

-- List all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- View table structure
sp_help 'Products'
```

### Backup Database
```sql
BACKUP DATABASE WMSDB
TO DISK = 'C:\Backup\WMSDB.bak'
WITH FORMAT;
```

### Restore Database
```sql
RESTORE DATABASE WMSDB
FROM DISK = 'C:\Backup\WMSDB.bak'
WITH REPLACE;
```

## Next Steps

After database setup:
1. ? Database created on CONGTAM-PC
2. ? Tables and relationships established
3. ? Seed data populated
4. Run all microservices: `.\run-all-services.ps1`
5. Test authentication with admin credentials
6. Access Swagger UI for each service
7. Test database connectivity through APIs

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [SQL Server Documentation](https://docs.microsoft.com/sql/)
- [WMS Project Documentation](./README.md)
- [Microservices Architecture](./MICROSERVICES_ARCHITECTURE.md)
