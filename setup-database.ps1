# Database Setup Script for WMS
# This script creates the database using EF Core migrations on CONGTAM-PC SQL Server

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "   WMS Database Setup Script" -ForegroundColor Cyan
Write-Host "   SQL Server: CONGTAM-PC" -ForegroundColor Cyan
Write-Host "   Database: WMSDB" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Check if EF Core tools are installed
Write-Host "Checking if Entity Framework Core tools are installed..." -ForegroundColor Yellow
$efTools = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efTools) {
    Write-Host "EF Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "EF Core tools installed successfully!" -ForegroundColor Green
} else {
    Write-Host "EF Core tools already installed." -ForegroundColor Green
    # Update to latest version
    Write-Host "Updating EF Core tools to latest version..." -ForegroundColor Yellow
    dotnet tool update --global dotnet-ef
}

Write-Host ""

# Navigate to Infrastructure project
Write-Host "Navigating to WMS.Infrastructure project..." -ForegroundColor Yellow
$infraPath = Join-Path $PSScriptRoot "WMS.Infrastructure"

if (-not (Test-Path $infraPath)) {
    Write-Host "ERROR: WMS.Infrastructure project not found at: $infraPath" -ForegroundColor Red
    exit 1
}

Set-Location $infraPath
Write-Host "Current directory: $infraPath" -ForegroundColor Green
Write-Host ""

# Check for existing migrations
Write-Host "Checking for existing migrations..." -ForegroundColor Yellow
$migrationsPath = Join-Path $infraPath "Migrations"

if (Test-Path $migrationsPath) {
    $migrationFiles = Get-ChildItem -Path $migrationsPath -Filter "*.cs" | Where-Object { $_.Name -notlike "*Designer.cs" -and $_.Name -ne "WMSDbContextModelSnapshot.cs" }
    
    if ($migrationFiles.Count -gt 0) {
        Write-Host "Found $($migrationFiles.Count) existing migration(s):" -ForegroundColor Green
        $migrationFiles | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
        Write-Host ""
        
        $response = Read-Host "Do you want to remove existing migrations and create new ones? (y/n)"
        if ($response -eq 'y' -or $response -eq 'Y') {
            Write-Host "Removing existing migrations..." -ForegroundColor Yellow
            Remove-Item -Path $migrationsPath -Recurse -Force
            Write-Host "Existing migrations removed." -ForegroundColor Green
            Write-Host ""
            
            Write-Host "Creating new initial migration..." -ForegroundColor Yellow
            dotnet ef migrations add InitialCreate --startup-project ..\WMS.Auth.API
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Migration created successfully!" -ForegroundColor Green
            } else {
                Write-Host "ERROR: Failed to create migration" -ForegroundColor Red
                exit 1
            }
        }
    }
} else {
    Write-Host "No existing migrations found. Creating initial migration..." -ForegroundColor Yellow
    dotnet ef migrations add InitialCreate --startup-project ..\WMS.Auth.API
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration created successfully!" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to create migration" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Apply migrations to database
Write-Host "Applying migrations to database (CONGTAM-PC/WMSDB)..." -ForegroundColor Yellow
dotnet ef database update --startup-project ..\WMS.Auth.API

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host "   Database setup completed successfully!" -ForegroundColor Green
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Details:" -ForegroundColor Cyan
    Write-Host "  Server: CONGTAM-PC" -ForegroundColor White
    Write-Host "  Database: WMSDB" -ForegroundColor White
    Write-Host "  Authentication: Windows Authentication" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Admin Credentials:" -ForegroundColor Cyan
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Password: Admin@123" -ForegroundColor White
    Write-Host ""
    Write-Host "Database Tables Created:" -ForegroundColor Cyan
    Write-Host "  - Users, Roles, UserRoles (Authentication)" -ForegroundColor White
    Write-Host "  - Products (Product Master Data)" -ForegroundColor White
    Write-Host "  - Locations (Warehouse Locations)" -ForegroundColor White
    Write-Host "  - Inventories (Stock Levels)" -ForegroundColor White
    Write-Host "  - InventoryTransactions (Stock Movements)" -ForegroundColor White
    Write-Host "  - Inbounds, InboundItems (Receiving)" -ForegroundColor White
    Write-Host "  - Outbounds, OutboundItems (Shipping)" -ForegroundColor White
    Write-Host "  - Payments, PaymentEvents (Payment Processing)" -ForegroundColor White
    Write-Host "  - Deliveries, DeliveryEvents (Delivery Tracking)" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "===============================================" -ForegroundColor Red
    Write-Host "   ERROR: Database setup failed!" -ForegroundColor Red
    Write-Host "===============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Verify SQL Server is running on CONGTAM-PC" -ForegroundColor White
    Write-Host "2. Ensure Windows Authentication is enabled" -ForegroundColor White
    Write-Host "3. Check that your Windows user has permissions to create databases" -ForegroundColor White
    Write-Host "4. Try connecting to SQL Server using SSMS to verify connectivity" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Return to original directory
Set-Location $PSScriptRoot

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
