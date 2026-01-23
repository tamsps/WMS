# WMS Microservices Setup Script
# This script creates all microservice projects and configures them

Write-Host "================================" -ForegroundColor Cyan
Write-Host "WMS Microservices Setup" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Define microservices
$microservices = @(
    @{Name="WMS.Auth.API"; Port=5001; Description="Authentication Service"},
    @{Name="WMS.Products.API"; Port=5002; Description="Product Management Service"},
    @{Name="WMS.Locations.API"; Port=5003; Description="Location Management Service"},
    @{Name="WMS.Inventory.API"; Port=5004; Description="Inventory Management Service"},
    @{Name="WMS.Inbound.API"; Port=5005; Description="Inbound Operations Service"},
    @{Name="WMS.Outbound.API"; Port=5006; Description="Outbound Operations Service"},
    @{Name="WMS.Payment.API"; Port=5007; Description="Payment Management Service"},
    @{Name="WMS.Delivery.API"; Port=5008; Description="Delivery Management Service"}
)

# Create microservices
foreach ($service in $microservices) {
    Write-Host "Creating $($service.Name) - $($service.Description)..." -ForegroundColor Yellow
    
    # Create new Web API project
    dotnet new webapi -n $service.Name -o $service.Name --framework net9.0 --no-https false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Created $($service.Name)" -ForegroundColor Green
        
        # Add project references
        Write-Host "  Adding project references..." -ForegroundColor Gray
        dotnet add "$($service.Name)/$($service.Name).csproj" reference WMS.Application/WMS.Application.csproj
        dotnet add "$($service.Name)/$($service.Name).csproj" reference WMS.Infrastructure/WMS.Infrastructure.csproj
        dotnet add "$($service.Name)/$($service.Name).csproj" reference WMS.Domain/WMS.Domain.csproj
        
        # Add required NuGet packages
        Write-Host "  Adding NuGet packages..." -ForegroundColor Gray
        dotnet add "$($service.Name)/$($service.Name).csproj" package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
        dotnet add "$($service.Name)/$($service.Name).csproj" package Microsoft.EntityFrameworkCore.Design --version 9.0.0
        dotnet add "$($service.Name)/$($service.Name).csproj" package Swashbuckle.AspNetCore --version 7.0.5
        
        Write-Host "? Configured $($service.Name)" -ForegroundColor Green
    } else {
        Write-Host "? Failed to create $($service.Name)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review MICROSERVICES_ARCHITECTURE.md for architecture details"
Write-Host "2. Update Program.cs in each microservice"
Write-Host "3. Copy corresponding controllers from WMS.API"
Write-Host "4. Test each microservice independently"
Write-Host ""
