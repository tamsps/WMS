# WMS Microservices - Quick Start Guide

## ?? Get Started in 3 Steps

### Step 1: Verify Prerequisites
```powershell
# Check .NET version
dotnet --version  # Should be 9.0 or higher

# Check SQL Server
# Ensure SQL Server LocalDB or SQL Server is running
```

### Step 2: Build All Services
```powershell
dotnet build
```

### Step 3: Run All Services
```powershell
# Option A: Use the automated script (Windows)
.\run-all-services.ps1

# Option B: Run manually (any OS)
# Open 8 terminals and run:
dotnet run --project WMS.Auth.API --urls=https://localhost:5001
dotnet run --project WMS.Products.API --urls=https://localhost:5002
dotnet run --project WMS.Locations.API --urls=https://localhost:5003
dotnet run --project WMS.Inventory.API --urls=https://localhost:5004
dotnet run --project WMS.Inbound.API --urls=https://localhost:5005
dotnet run --project WMS.Outbound.API --urls=https://localhost:5006
dotnet run --project WMS.Payment.API --urls=https://localhost:5007
dotnet run --project WMS.Delivery.API --urls=https://localhost:5008
```

## ?? Test the Services

### 1. Open Swagger UIs
- Auth: https://localhost:5001
- Products: https://localhost:5002
- Locations: https://localhost:5003
- Inventory: https://localhost:5004
- Inbound: https://localhost:5005
- Outbound: https://localhost:5006
- Payment: https://localhost:5007
- Delivery: https://localhost:5008

### 2. Login to Get Token
```bash
POST https://localhost:5001/api/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

Copy the `token` from the response.

### 3. Test Other Services
Use the token in Swagger UI:
1. Click "Authorize" button
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"
4. Try any endpoint

## ?? Documentation

- **Full Architecture**: See `MICROSERVICES_ARCHITECTURE.md`
- **Running Guide**: See `RUN_MICROSERVICES.md`
- **Complete Summary**: See `REFACTORING_SUMMARY.md`
- **User Guide**: See `USER_GUIDE.md`

## ?? What Changed?

### Before (Monolith)
```
WMS.API/
??? Controllers/
    ??? AuthController.cs
    ??? ProductsController.cs
    ??? LocationsController.cs
    ??? InventoryController.cs
    ??? InboundController.cs
    ??? OutboundController.cs
    ??? PaymentController.cs
    ??? DeliveryController.cs
```

### After (Microservices)
```
WMS.Auth.API/        ? Port 5001
WMS.Products.API/    ? Port 5002
WMS.Locations.API/   ? Port 5003
WMS.Inventory.API/   ? Port 5004
WMS.Inbound.API/     ? Port 5005
WMS.Outbound.API/    ? Port 5006
WMS.Payment.API/     ? Port 5007
WMS.Delivery.API/    ? Port 5008
```

## ? What's Included

- ? 8 fully functional microservices
- ? JWT authentication across all services
- ? Swagger UI for each service
- ? Shared database configuration
- ? CORS enabled
- ? Complete documentation
- ? Docker Compose file
- ? PowerShell run scripts

## ?? Common Commands

```powershell
# Build all services
dotnet build

# Run a specific service
dotnet run --project WMS.Auth.API --urls=https://localhost:5001

# Watch mode (auto-reload)
dotnet watch run --project WMS.Auth.API --urls=https://localhost:5001

# Run with Docker
docker-compose up

# Clean build
dotnet clean
dotnet restore
dotnet build
```

## ?? Troubleshooting

### "Port already in use"
Change the port:
```powershell
dotnet run --project WMS.Auth.API --urls=https://localhost:XXXX
```

### "Cannot connect to database"
1. Check SQL Server is running
2. Verify connection string in `appsettings.json`
3. Run migrations:
```powershell
cd WMS.Infrastructure
dotnet ef database update --startup-project ../WMS.Auth.API
```

### "Unauthorized" errors
Ensure you're using a valid JWT token:
1. Login via Auth API
2. Copy the token
3. Add to requests: `Authorization: Bearer {token}`

## ?? Need Help?

1. Check the documentation files
2. View Swagger UI for API details
3. Check service logs in the console

---

**?? You're ready to go! All microservices are operational.**

**Default Login**: `admin` / `Admin@123`

Happy coding! ??
