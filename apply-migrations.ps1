# Apply Database Migrations to WMSDB
# This script applies existing EF Core migrations to create tables in WMSDB database

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "   Applying Database Migrations to WMSDB" -ForegroundColor Cyan
Write-Host "   SQL Server: CONGTAM-PC" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to the root directory
$rootPath = $PSScriptRoot
Set-Location $rootPath

Write-Host "Checking if dotnet-ef tool is installed..." -ForegroundColor Yellow
$efTools = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efTools) {
    Write-Host "Installing Entity Framework Core tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install EF Core tools" -ForegroundColor Red
        exit 1
    }
    Write-Host "EF Core tools installed successfully!" -ForegroundColor Green
} else {
    Write-Host "EF Core tools found. Updating to latest version..." -ForegroundColor Yellow
    dotnet tool update --global dotnet-ef
}

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "Applying migrations to WMSDB database..." -ForegroundColor Yellow
Write-Host "Connection: Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True" -ForegroundColor Gray
Write-Host ""

# Navigate to Infrastructure project
Set-Location "WMS.Infrastructure"

# Apply migrations
dotnet ef database update --startup-project ..\WMS.Auth.API --verbose

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host "   Database tables created successfully!" -ForegroundColor Green
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Details:" -ForegroundColor Cyan
    Write-Host "  Server: CONGTAM-PC" -ForegroundColor White
    Write-Host "  Database: WMSDB" -ForegroundColor White
    Write-Host "  Authentication: Windows Authentication" -ForegroundColor White
    Write-Host ""
    Write-Host "Tables Created:" -ForegroundColor Cyan
    Write-Host "  ? Users, Roles, UserRoles" -ForegroundColor White
    Write-Host "  ? Products" -ForegroundColor White
    Write-Host "  ? Locations" -ForegroundColor White
    Write-Host "  ? Inventories, InventoryTransactions" -ForegroundColor White
    Write-Host "  ? Inbounds, InboundItems" -ForegroundColor White
    Write-Host "  ? Outbounds, OutboundItems" -ForegroundColor White
    Write-Host "  ? Payments, PaymentEvents" -ForegroundColor White
    Write-Host "  ? Deliveries, DeliveryEvents" -ForegroundColor White
    Write-Host ""
    Write-Host "Seed Data:" -ForegroundColor Cyan
    Write-Host "  ? 3 Roles: Admin, Manager, WarehouseStaff" -ForegroundColor White
    Write-Host "  ? 1 Admin User" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Admin Credentials:" -ForegroundColor Yellow
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: Admin@123" -ForegroundColor White
    Write-Host "  Email: admin@wms.com" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "===============================================" -ForegroundColor Red
    Write-Host "   ERROR: Migration failed!" -ForegroundColor Red
    Write-Host "===============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Verify SQL Server is running on CONGTAM-PC" -ForegroundColor White
    Write-Host "2. Ensure WMSDB database exists (even if empty)" -ForegroundColor White
    Write-Host "3. Check Windows Authentication permissions" -ForegroundColor White
    Write-Host "4. Try connecting using SSMS first to verify connectivity" -ForegroundColor White
    Write-Host ""
    
    # Return to root
    Set-Location $rootPath
    exit 1
}

# Return to root directory
Set-Location $rootPath

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
