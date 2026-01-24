# WMS Migration Helper Script - Updated for WMS.Web Startup
# Makes creating and applying migrations easier

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("add", "update", "list", "remove", "script", "drop")]
    [string]$Action = "add",
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Web", "Auth", "Products", "Locations", "Inventory", "Inbound", "Outbound", "Payment", "Delivery")]
    [string]$StartupProject = "Web"  # ? Changed default to Web
)

$ErrorActionPreference = "Stop"

# Configuration
$DomainProject = "WMS.Domain"
$StartupProjectPath = if ($StartupProject -eq "Web") { "WMS.Web" } else { "WMS.$StartupProject.API" }

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?        WMS Database Migration Helper                ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
Write-Host "Startup Project: " -ForegroundColor Gray -NoNewline
Write-Host "$StartupProjectPath" -ForegroundColor Yellow
Write-Host ""

function Show-Usage {
    Write-Host "Usage Examples:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  # Create a new migration" -ForegroundColor Green
    Write-Host "  .\migrate.ps1 -Action add -MigrationName AddProductCategory"
    Write-Host ""
    Write-Host "  # Apply migrations to database" -ForegroundColor Green
    Write-Host "  .\migrate.ps1 -Action update"
    Write-Host ""
    Write-Host "  # List all migrations" -ForegroundColor Green
    Write-Host "  .\migrate.ps1 -Action list"
    Write-Host ""
    Write-Host "  # Remove last migration (if not applied)" -ForegroundColor Green
    Write-Host "  .\migrate.ps1 -Action remove"
    Write-Host ""
    Write-Host "  # Generate SQL script" -ForegroundColor Green
    Write-Host "  .\migrate.ps1 -Action script"
    Write-Host ""
    Write-Host "  # Drop database" -ForegroundColor Red
    Write-Host "  .\migrate.ps1 -Action drop"
    Write-Host ""
    Write-Host "  # Use different startup project" -ForegroundColor Cyan
    Write-Host "  .\migrate.ps1 -Action update -StartupProject Auth"
    Write-Host ""
}

function Test-Prerequisites {
    # Check if dotnet ef is installed
    try {
        $efVersion = dotnet ef --version 2>&1
        Write-Host "? EF Core tools installed: " -ForegroundColor Green -NoNewline
        Write-Host $efVersion
    }
    catch {
        Write-Host "? EF Core tools not installed!" -ForegroundColor Red
        Write-Host "Install with: dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
        exit 1
    }

    # Check if projects exist
    if (-not (Test-Path $DomainProject)) {
        Write-Host "? $DomainProject project not found!" -ForegroundColor Red
        exit 1
    }

    if (-not (Test-Path $StartupProjectPath)) {
        Write-Host "? $StartupProjectPath project not found!" -ForegroundColor Red
        Write-Host "Available startup projects: Web, Auth, Products, Locations, Inventory, Inbound, Outbound, Payment, Delivery" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "? Projects found" -ForegroundColor Green
    Write-Host ""
}

function Add-Migration {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Host "? Migration name is required!" -ForegroundColor Red
        Write-Host "Usage: .\migrate.ps1 -Action add -MigrationName YourMigrationName" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan
    Write-Host "  Domain Project: $DomainProject" -ForegroundColor Gray
    Write-Host "  Startup Project: $StartupProjectPath" -ForegroundColor Gray
    Write-Host ""

    try {
        dotnet ef migrations add $MigrationName `
            --project $DomainProject `
            --startup-project $StartupProjectPath `
            --context WMSDbContext `
            --verbose

        Write-Host ""
        Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "?  ? Migration created successfully!                  ?" -ForegroundColor Green
        Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host ""
        Write-Host "Migration files created in: $DomainProject\Migrations\" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Review the generated migration file" -ForegroundColor White
        Write-Host "  2. Apply to database: .\migrate.ps1 -Action update" -ForegroundColor White
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to create migration!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

function Update-Database {
    Write-Host "Applying migrations to database..." -ForegroundColor Cyan
    Write-Host "  Domain Project: $DomainProject" -ForegroundColor Gray
    Write-Host "  Startup Project: $StartupProjectPath" -ForegroundColor Gray
    Write-Host ""

    try {
        dotnet ef database update `
            --project $DomainProject `
            --startup-project $StartupProjectPath `
            --verbose

        Write-Host ""
        Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "?  ? Database updated successfully!                   ?" -ForegroundColor Green
        Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to update database!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

function List-Migrations {
    Write-Host "Listing all migrations..." -ForegroundColor Cyan
    Write-Host ""

    try {
        dotnet ef migrations list `
            --project $DomainProject `
            --startup-project $StartupProjectPath

        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to list migrations!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

function Remove-LastMigration {
    Write-Host "Removing last migration..." -ForegroundColor Yellow
    Write-Host "WARNING: This will delete migration files!" -ForegroundColor Red
    Write-Host ""

    $confirmation = Read-Host "Are you sure? (yes/no)"
    if ($confirmation -ne "yes") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }

    try {
        dotnet ef migrations remove `
            --project $DomainProject `
            --startup-project $StartupProjectPath `
            --force

        Write-Host ""
        Write-Host "? Migration removed successfully!" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to remove migration!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Note: You can only remove migrations that haven't been applied to the database." -ForegroundColor Yellow
        exit 1
    }
}

function Export-SqlScript {
    $scriptFile = "migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    
    Write-Host "Generating SQL script..." -ForegroundColor Cyan
    Write-Host "  Output: $scriptFile" -ForegroundColor Gray
    Write-Host ""

    try {
        dotnet ef migrations script `
            --project $DomainProject `
            --startup-project $StartupProjectPath `
            --output $scriptFile `
            --idempotent

        Write-Host ""
        Write-Host "? SQL script generated: $scriptFile" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to generate SQL script!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

function Drop-Database {
    Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host "?  ??  WARNING: DROP DATABASE                         ?" -ForegroundColor Red
    Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Red
    Write-Host ""
    Write-Host "This will PERMANENTLY DELETE the entire database!" -ForegroundColor Red
    Write-Host "All data will be lost!" -ForegroundColor Red
    Write-Host ""

    $confirmation = Read-Host "Type 'DELETE' to confirm"
    if ($confirmation -ne "DELETE") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit 0
    }

    try {
        dotnet ef database drop `
            --project $DomainProject `
            --startup-project $StartupProjectPath `
            --force

        Write-Host ""
        Write-Host "? Database dropped successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "To recreate: .\migrate.ps1 -Action update" -ForegroundColor Yellow
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "? Failed to drop database!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}

# Main execution
try {
    Test-Prerequisites

    switch ($Action) {
        "add" { Add-Migration }
        "update" { Update-Database }
        "list" { List-Migrations }
        "remove" { Remove-LastMigration }
        "script" { Export-SqlScript }
        "drop" { Drop-Database }
        default { Show-Usage }
    }
}
catch {
    Write-Host ""
    Write-Host "? An error occurred!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "For help, run: .\migrate.ps1" -ForegroundColor Yellow
    exit 1
}
