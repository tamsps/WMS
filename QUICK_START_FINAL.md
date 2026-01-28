# WMS - Quick Start Guide

## Overview
This document provides a quick reference for getting the WMS system up and running.

## Prerequisites
- Windows 10+ or Windows Server 2019+
- .NET 9 SDK installed
- SQL Server LocalDB or SQL Server 2019+
- PowerShell 5.1+ or Command Prompt
- Administrator privileges (for some operations)

## Quick Start in 3 Steps

### Step 1: Setup Database (5 minutes)
```batch
DATABASE_SETUP.bat
```
This will:
- Build the solution
- Create WMSDB database
- Apply all migrations
- Seed initial data

### Step 2: Start All Services (2 minutes)
```batch
START_ALL_SERVICES.bat
```
This will open 11 command windows, each running one service.

### Step 3: Access the Application
Open your browser and navigate to:
```
https://localhost:5001
```

## Default Login Credentials
```
Username: admin
Password: Admin@123
```

## Service Startup Ports

| Service | Port | URL |
|---------|------|-----|
| Web UI | 5001 | https://localhost:5001 |
| API Gateway | 5000 | https://localhost:5000 |
| WMS.API | 5011 | https://localhost:5011 |
| WMS.Auth.API | 5002 | https://localhost:5002 |
| WMS.Products.API | 5003 | https://localhost:5003 |
| WMS.Locations.API | 5004 | https://localhost:5004 |
| WMS.Inbound.API | 5005 | https://localhost:5005 |
| WMS.Outbound.API | 5006 | https://localhost:5006 |
| WMS.Payment.API | 5007 | https://localhost:5007 |
| WMS.Delivery.API | 5009 | https://localhost:5009 |
| WMS.Inventory.API | 5010 | https://localhost:5010 |

## Useful Links

| Purpose | URL |
|---------|-----|
| Web Application | https://localhost:5001 |
| API Gateway | https://localhost:5000 |
| Swagger API Docs | https://localhost:5000/swagger |
| Gateway Health | https://localhost:5000/health |
| API Health | https://localhost:5011/health |

## Available Batch Scripts

### START_ALL_SERVICES.bat
Starts all 11 services in separate command windows
```batch
START_ALL_SERVICES.bat
```

### STOP_ALL_SERVICES.bat
Stops all running WMS services (requires Admin)
```batch
STOP_ALL_SERVICES.bat
```

### DATABASE_SETUP.bat
Sets up the database with migrations and seed data
```batch
DATABASE_SETUP.bat
```

### HEALTH_CHECK.bat
Checks if all services are running
```batch
HEALTH_CHECK.bat
```

## Troubleshooting

### Port Already in Use
```powershell
# Find process on port 5001
netstat -ano | findstr :5001

# Kill the process (replace PID)
taskkill /PID <PID> /F
```

### Database Connection Failed
```powershell
# Start SQL Server LocalDB
sqllocaldb start mssqllocaldb

# Check if database exists
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases WHERE name='WMSDB'"
```

### Can't Login
1. Ensure DATABASE_SETUP.bat was run
2. Verify Auth.API is running on port 5002
3. Clear browser cookies (Ctrl+Shift+Delete)
4. Try incognito/private window

### Service Won't Start
1. Check if port is available
2. Run `dotnet restore` in the service directory
3. Run `dotnet build` to check for compilation errors
4. Check .NET 9 SDK is installed: `dotnet --version`

## Full Documentation

For complete information, see: **ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md**

Topics covered:
- System architecture overview
- Service responsibilities
- Complete API specifications
- Database schema details
- Configuration instructions
- Advanced troubleshooting
- Performance optimization
- Security best practices
- Production deployment options

## Project Structure

```
WMS/
├── WMS.sln                    (Solution file)
├── WMS.API/                   (Main API backend)
├── WMS.Web/                   (Web UI - MVC)
├── WMS.Gateway/               (API Gateway - YARP)
├── WMS.Auth.API/              (Authentication)
├── WMS.Products.API/          (Product service)
├── WMS.Locations.API/         (Location service)
├── WMS.Inbound.API/           (Inbound operations)
├── WMS.Outbound.API/          (Outbound operations)
├── WMS.Payment.API/           (Payment service)
├── WMS.Delivery.API/          (Delivery service)
├── WMS.Inventory.API/         (Inventory service)
├── WMS.Domain/                (Shared domain models)
├── WMS.Application/           (Shared DTOs)
├── WMS.Infrastructure/        (Data access layer)
├── START_ALL_SERVICES.bat     (Start all services)
├── STOP_ALL_SERVICES.bat      (Stop all services)
├── DATABASE_SETUP.bat         (Initialize database)
├── HEALTH_CHECK.bat           (Check service health)
└── ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md
```

## Key Modules

### 1. Product Module
- Product CRUD operations
- SKU management
- Product categorization
- Activation/Deactivation

### 2. Location Module
- Warehouse locations
- Hierarchical structure (Zone → Rack → Bin)
- Capacity management
- Location inventory tracking

### 3. Inventory Module
- Real-time stock levels
- Stock movements tracking
- Low stock alerts
- Inventory reconciliation

### 4. Inbound Module
- Receiving orders
- Multi-step receiving workflow
- Item-level tracking
- Quality checks

### 5. Outbound Module
- Customer orders
- Picking workflow
- Shipping confirmation
- Tracking number generation

### 6. Payment Module
- Transaction management
- Multiple payment methods
- Multi-currency support
- Payment verification

### 7. Delivery Module
- Delivery tracking
- Shipment status updates
- Carrier management
- Public tracking page (no login required)

## System Architecture

```
[Web UI: 5001]  [Mobile App]
      ↓              ↓
[API Gateway: 5000]
      ↓
[Multiple Microservices]
      ↓
[SQL Server LocalDB: WMSDB]
```

## Development Workflow

1. **Make code changes** in Visual Studio
2. **Build solution** (F7 or Ctrl+Shift+B)
3. **Stop services** with STOP_ALL_SERVICES.bat
4. **Start services** with START_ALL_SERVICES.bat
5. **Test changes** in Web UI or via API

## API Testing

Use Postman or curl to test APIs:

```bash
# Login
curl -X POST https://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}' \
  -k

# Get Products (requires Bearer token from login response)
curl -X GET https://localhost:5000/api/products \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -k
```

## Environment Variables (Optional)

For production, use environment variables instead of appsettings.json:

```powershell
# Set variables
$env:ConnectionStrings__DefaultConnection="Server=...;Database=WMSDB;..."
$env:JwtSettings__SecretKey="YourSecretKey..."

# Run service
dotnet run
```

## Next Steps

1. ✅ Run DATABASE_SETUP.bat
2. ✅ Run START_ALL_SERVICES.bat
3. ✅ Open https://localhost:5001
4. ✅ Login with admin/Admin@123
5. ✅ Explore all 7 modules
6. ✅ Test CRUD operations
7. ✅ Review complete guide: ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md

## Support

For detailed information on:
- **Architecture**: See ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md - Architecture System
- **APIs**: See ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md - API Specifications
- **Database**: See ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md - Database Deployment
- **Deployment**: See ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md - Deployment section

## Status

✅ **Production Ready**
- All 11 services operational
- Complete Web UI (7 modules)
- All microservices configured
- API Gateway routing all requests
- Database migrations applied
- Seed data populated
- Authentication and authorization working

**Last Updated**: January 28, 2026
