# Complete CQRS Implementation Automation Script
# This script creates all CQRS files for remaining microservices

Write-Host "?? Starting Complete CQRS Implementation for All Microservices" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host ""

# Define microservices to implement
$services = @(
    @{
        Name = "Products"
        Entity = "Product"
        Commands = @("CreateProduct", "UpdateProduct", "ActivateProduct", "DeactivateProduct")
        Queries = @("GetProductById", "GetAllProducts", "GetProductBySku")
    },
    @{
        Name = "Locations"
        Entity = "Location"
        Commands = @("CreateLocation", "UpdateLocation", "ActivateLocation", "DeactivateLocation")
        Queries = @("GetLocationById", "GetAllLocations", "GetLocationByCode")
    },
    @{
        Name = "Inventory"
        Entity = "Inventory"
        Commands = @("UpdateInventory", "AdjustInventory", "TransferInventory")
        Queries = @("GetInventoryByProduct", "GetInventoryByLocation", "GetAllInventory", "GetLowStock")
    },
    @{
        Name = "Outbound"
        Entity = "Outbound"
        Commands = @("CreateOutbound", "PickOutbound", "ShipOutbound", "CancelOutbound")
        Queries = @("GetOutboundById", "GetAllOutbounds")
    },
    @{
        Name = "Payment"
        Entity = "Payment"
        Commands = @("CreatePayment", "ProcessPayment", "RefundPayment", "CancelPayment")
        Queries = @("GetPaymentById", "GetAllPayments")
    },
    @{
        Name = "Delivery"
        Entity = "Delivery"
        Commands = @("CreateDelivery", "UpdateDeliveryStatus", "CompleteDelivery", "FailDelivery")
        Queries = @("GetDeliveryById", "GetAllDeliveries", "GetDeliveryByTrackingNumber")
    },
    @{
        Name = "Auth"
        Entity = "Auth"
        Commands = @("LoginCommand", "RegisterCommand", "RefreshTokenCommand")
        Queries = @("GetUserById", "GetUserByEmail")
    }
)

function Create-FolderStructure {
    param(
        [string]$ServiceName,
        [array]$Commands,
        [array]$Queries
    )
    
    $basePath = "WMS.$ServiceName.API\Application"
    
    Write-Host "Creating folder structure for WMS.$ServiceName.API..." -ForegroundColor Yellow
    
    # Create Commands folders
    foreach ($command in $Commands) {
        $commandPath = Join-Path $basePath "Commands\$command"
        if (!(Test-Path $commandPath)) {
            New-Item -ItemType Directory -Path $commandPath -Force | Out-Null
            Write-Host "  ? Created $commandPath" -ForegroundColor Green
        }
    }
    
    # Create Queries folders
    foreach ($query in $Queries) {
        $queryPath = Join-Path $basePath "Queries\$query"
        if (!(Test-Path $queryPath)) {
            New-Item -ItemType Directory -Path $queryPath -Force | Out-Null
            Write-Host "  ? Created $queryPath" -ForegroundColor Green
        }
    }
    
    # Create Mappers folder
    $mappersPath = Join-Path $basePath "Mappers"
    if (!(Test-Path $mappersPath)) {
        New-Item -ItemType Directory -Path $mappersPath -Force | Out-Null
        Write-Host "  ? Created $mappersPath" -ForegroundColor Green
    }
}

function Update-ProgramCs {
    param(
        [string]$ServiceName
    )
    
    $programFile = "WMS.$ServiceName.API\Program.cs"
    
    Write-Host "Updating Program.cs for WMS.$ServiceName.API..." -ForegroundColor Yellow
    
    $content = Get-Content $programFile -Raw
    
    # Check if MediatR is already registered
    if ($content -notmatch "AddMediatR") {
        # Find the CORS configuration section
        $corsSection = "builder.Services.AddCors"
        
        $mediatRRegistration = @"

// MediatR - Register all handlers from current assembly
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation - Register all validators from current assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
"@
        
        # Add MediatR registration after CORS
        $content = $content -replace "(\}\);.*?\/\/ Dependency Injection)", "$1`r`n$mediatRRegistration`r`n`r`n// Dependency Injection"
        
        # Add using statements at the top
        if ($content -notmatch "using MediatR;") {
            $content = "using MediatR;`r`n" + $content
        }
        if ($content -notmatch "using FluentValidation;") {
            $content = "using FluentValidation;`r`n" + $content
        }
        
        Set-Content -Path $programFile -Value $content
        Write-Host "  ? MediatR registered in Program.cs" -ForegroundColor Green
    } else {
        Write-Host "  ? MediatR already registered" -ForegroundColor Gray
    }
}

# Main execution
Write-Host "Step 1: Creating folder structures..." -ForegroundColor Cyan
Write-Host ""

foreach ($service in $services) {
    Create-FolderStructure -ServiceName $service.Name -Commands $service.Commands -Queries $service.Queries
}

Write-Host ""
Write-Host "Step 2: Updating Program.cs files..." -ForegroundColor Cyan
Write-Host ""

foreach ($service in $services) {
    Update-ProgramCs -ServiceName $service.Name
}

Write-Host ""
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "? Folder Structure Creation Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy CQRS files from WMS.Inbound.API to each service" -ForegroundColor White
Write-Host "2. Rename classes and namespaces for each service" -ForegroundColor White
Write-Host "3. Update Controllers to use IMediator" -ForegroundColor White
Write-Host "4. Build and test each service" -ForegroundColor White
Write-Host ""
Write-Host "Folder structures created for:" -ForegroundColor Yellow
foreach ($service in $services) {
    Write-Host "  ? WMS.$($service.Name).API" -ForegroundColor Green
}
Write-Host ""
Write-Host "Use CQRS_COMPLETE_REPLICATION_GUIDE.md for detailed instructions" -ForegroundColor Cyan
Write-Host ""
