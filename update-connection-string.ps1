# Update Connection String Across All Microservices
# This script updates the connection string in all appsettings.json files

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "CONGTAM-PC",
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "WMSDB",
    
    [Parameter(Mandatory=$false)]
    [string]$CustomConnectionString
)

$ErrorActionPreference = "Stop"

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   WMS Connection String Updater                     ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Build connection string
if ($CustomConnectionString) {
    $connectionString = $CustomConnectionString
    Write-Host "Using custom connection string" -ForegroundColor Yellow
} else {
    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
    Write-Host "Server: $Server" -ForegroundColor Gray
    Write-Host "Database: $Database" -ForegroundColor Gray
}

Write-Host ""
Write-Host "New Connection String:" -ForegroundColor Cyan
Write-Host $connectionString -ForegroundColor White
Write-Host ""

# Find all appsettings.json in API projects
$apiProjects = @(
    "WMS.Auth.API",
    "WMS.Products.API",
    "WMS.Locations.API",
    "WMS.Inventory.API",
    "WMS.Inbound.API",
    "WMS.Outbound.API",
    "WMS.Payment.API",
    "WMS.Delivery.API"
)

$updatedCount = 0
$failedFiles = @()

Write-Host "Updating connection strings..." -ForegroundColor Cyan
Write-Host ""

foreach ($project in $apiProjects) {
    $appsettingsPath = Join-Path $project "appsettings.json"
    
    if (Test-Path $appsettingsPath) {
        try {
            # Read JSON file
            $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
            
            # Update connection string
            if (-not $json.ConnectionStrings) {
                $json | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
            }
            
            $json.ConnectionStrings.DefaultConnection = $connectionString
            
            # Write back to file with proper formatting
            $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
            
            Write-Host "? Updated: $project" -ForegroundColor Green
            $updatedCount++
        }
        catch {
            Write-Host "? Failed: $project - $($_.Exception.Message)" -ForegroundColor Red
            $failedFiles += $project
        }
    }
    else {
        Write-Host "? Skipped: $project (file not found)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  Update Complete!                                   ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Successfully updated: $updatedCount files" -ForegroundColor Green

if ($failedFiles.Count -gt 0) {
    Write-Host "Failed to update: $($failedFiles.Count) files" -ForegroundColor Red
    foreach ($file in $failedFiles) {
        Write-Host "  - $file" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Verify connection: Test-Path 'sqlserver://CONGTAM-PC'" -ForegroundColor White
Write-Host "  2. Create database: .\migrate.ps1 -Action update" -ForegroundColor White
Write-Host "  3. Start services: .\run-all-services.ps1" -ForegroundColor White
Write-Host ""
