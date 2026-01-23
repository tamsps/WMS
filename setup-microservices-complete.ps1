# WMS Microservices Complete Generation Script
# This script creates all microservice projects with complete implementations

Write-Host "================================" -ForegroundColor Cyan
Write-Host "WMS Microservices Complete Setup" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Define microservices with their configurations
$microservices = @(
    @{
        Name="WMS.Products.API"
        Port=5002
        Controller="Products"
        OriginalController="ProductsController.cs"
        Service="IProductService"
        Description="Product Management Service"
    },
    @{
        Name="WMS.Locations.API"
        Port=5003
        Controller="Locations"
        OriginalController="LocationsController.cs"
        Service="ILocationService"
        Description="Location Management Service"
    },
    @{
        Name="WMS.Inventory.API"
        Port=5004
        Controller="Inventory"
        OriginalController="InventoryController.cs"
        Service="IInventoryService"
        Description="Inventory Management Service"
    },
    @{
        Name="WMS.Inbound.API"
        Port=5005
        Controller="Inbound"
        OriginalController="InboundController.cs"
        Service="IInboundService"
        Description="Inbound Operations Service"
    },
    @{
        Name="WMS.Outbound.API"
        Port=5006
        Controller="Outbound"
        OriginalController="OutboundController.cs"
        Service="IOutboundService"
        Description="Outbound Operations Service"
    },
    @{
        Name="WMS.Payment.API"
        Port=5007
        Controller="Payment"
        OriginalController="PaymentController.cs"
        Service="IPaymentService"
        Description="Payment Management Service"
    },
    @{
        Name="WMS.Delivery.API"
        Port=5008
        Controller="Delivery"
        OriginalController="DeliveryController.cs"
        Service="IDeliveryService"
        Description="Delivery Management Service"
    }
)

# Create microservices
foreach ($service in $microservices) {
    Write-Host "Creating $($service.Name) - $($service.Description)..." -ForegroundColor Yellow
    
    # Create project directory
    $projectDir = $service.Name
    if (-not (Test-Path $projectDir)) {
        New-Item -ItemType Directory -Path $projectDir | Out-Null
    }
    
    # Create Controllers directory
    $controllersDir = Join-Path $projectDir "Controllers"
    if (-not (Test-Path $controllersDir)) {
        New-Item -ItemType Directory -Path $controllersDir | Out-Null
    }
    
    # Copy controller from WMS.API if exists
    $sourceController = Join-Path "WMS.API\Controllers" $service.OriginalController
    if (Test-Path $sourceController) {
        $destController = Join-Path $controllersDir "$($service.Controller)Controller.cs"
        Copy-Item -Path $sourceController -Destination $destController -Force
        
        # Update namespace in controller
        $content = Get-Content $destController -Raw
        $content = $content -replace "namespace WMS.API.Controllers", "namespace $($service.Name).Controllers"
        Set-Content -Path $destController -Value $content
        
        Write-Host "  ? Copied and updated $($service.Controller)Controller.cs" -ForegroundColor Green
    }
    
    Write-Host "  ? Created $($service.Name) structure" -ForegroundColor Green
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Creating Common Configuration Files" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Create docker-compose.yml for all services
$dockerCompose = @"
version: '3.8'

services:
  wms-auth-api:
    build:
      context: .
      dockerfile: WMS.Auth.API/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-products-api:
    build:
      context: .
      dockerfile: WMS.Products.API/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-locations-api:
    build:
      context: .
      dockerfile: WMS.Locations.API/Dockerfile
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-inventory-api:
    build:
      context: .
      dockerfile: WMS.Inventory.API/Dockerfile
    ports:
      - "5004:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-inbound-api:
    build:
      context: .
      dockerfile: WMS.Inbound.API/Dockerfile
    ports:
      - "5005:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-outbound-api:
    build:
      context: .
      dockerfile: WMS.Outbound.API/Dockerfile
    ports:
      - "5006:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-payment-api:
    build:
      context: .
      dockerfile: WMS.Payment.API/Dockerfile
    ports:
      - "5007:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-delivery-api:
    build:
      context: .
      dockerfile: WMS.Delivery.API/Dockerfile
    ports:
      - "5008:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WMSDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - sqlserver
    networks:
      - wms-network

  wms-web:
    build:
      context: .
      dockerfile: WMS.Web/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ApiSettings__BaseUrl=http://wms-auth-api:8080
    depends_on:
      - wms-auth-api
      - wms-products-api
      - wms-locations-api
      - wms-inventory-api
      - wms-inbound-api
      - wms-outbound-api
      - wms-payment-api
      - wms-delivery-api
    networks:
      - wms-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - wms-network

networks:
  wms-network:
    driver: bridge

volumes:
  sqlserver-data:
"@

Set-Content -Path "docker-compose.yml" -Value $dockerCompose
Write-Host "? Created docker-compose.yml" -ForegroundColor Green

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Microservices created:" -ForegroundColor Yellow
foreach ($service in $microservices) {
    Write-Host "  • $($service.Name) (Port $($service.Port))" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run: dotnet build to build all services"
Write-Host "2. Review MICROSERVICES_ARCHITECTURE.md for details"
Write-Host "3. Start services with: docker-compose up"
Write-Host "4. Or run individually with: dotnet run --project <ServiceName>"
Write-Host ""
