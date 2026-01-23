# Run All WMS Microservices
# This script starts all microservices in separate PowerShell windows

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Starting WMS Microservices" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$services = @(
    @{Name="Auth"; Path="WMS.Auth.API"; Port=5001; Color="Green"},
    @{Name="Products"; Path="WMS.Products.API"; Port=5002; Color="Blue"},
    @{Name="Locations"; Path="WMS.Locations.API"; Port=5003; Color="Yellow"},
    @{Name="Inventory"; Path="WMS.Inventory.API"; Port=5004; Color="Magenta"},
    @{Name="Inbound"; Path="WMS.Inbound.API"; Port=5005; Color="Cyan"},
    @{Name="Outbound"; Path="WMS.Outbound.API"; Port=5006; Color="Red"},
    @{Name="Payment"; Path="WMS.Payment.API"; Port=5007; Color="DarkGreen"},
    @{Name="Delivery"; Path="WMS.Delivery.API"; Port=5008; Color="DarkBlue"}
)

foreach ($service in $services) {
    Write-Host "Starting $($service.Name) API on port $($service.Port)..." -ForegroundColor $service.Color
    
    $command = "cd '$($service.Path)'; dotnet run --urls=https://localhost:$($service.Port)"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
    
    Start-Sleep -Seconds 2
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "All Services Started!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Yellow
foreach ($service in $services) {
    Write-Host "  $($service.Name) API: https://localhost:$($service.Port)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Press Ctrl+C in each window to stop the services" -ForegroundColor Yellow
Write-Host ""
Write-Host "To start the web application, run:" -ForegroundColor Yellow
Write-Host "  cd WMS.Web" -ForegroundColor Gray
Write-Host "  dotnet run --urls=https://localhost:5000" -ForegroundColor Gray
Write-Host ""
