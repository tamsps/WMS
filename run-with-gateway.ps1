# WMS - Run All Microservices with API Gateway
# PowerShell script to run all WMS microservices + API Gateway in separate windows

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  WMS Microservices + Gateway Launcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$services = @(
    @{ Name = "API Gateway"; Path = "WMS.Gateway"; Port = 5000; Color = "Magenta" },
    @{ Name = "Auth API"; Path = "WMS.Auth.API"; Port = 5001; Color = "Green" },
    @{ Name = "Products API"; Path = "WMS.Products.API"; Port = 5002; Color = "Yellow" },
    @{ Name = "Locations API"; Path = "WMS.Locations.API"; Port = 5003; Color = "Blue" },
    @{ Name = "Inbound API"; Path = "WMS.Inbound.API"; Port = 5004; Color = "Cyan" },
    @{ Name = "Outbound API"; Path = "WMS.Outbound.API"; Port = 5005; Color = "DarkYellow" },
    @{ Name = "Inventory API"; Path = "WMS.Inventory.API"; Port = 5006; Color = "DarkCyan" },
    @{ Name = "Payment API"; Path = "WMS.Payment.API"; Port = 5007; Color = "DarkGreen" },
    @{ Name = "Delivery API"; Path = "WMS.Delivery.API"; Port = 5008; Color = "DarkMagenta" }
)

# Function to check if a port is available
function Test-Port {
    param($Port)
    $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -InformationLevel Quiet
    return !$connection
}

# Function to kill process on a port
function Stop-ProcessOnPort {
    param($Port)
    try {
        $process = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
        if ($process) {
            Stop-Process -Id $process -Force -ErrorAction SilentlyContinue
            Write-Host "  Stopped process on port $Port" -ForegroundColor Gray
            Start-Sleep -Seconds 1
        }
    }
    catch {
        # Port not in use
    }
}

Write-Host "Step 1: Checking and freeing ports..." -ForegroundColor Cyan
foreach ($service in $services) {
    if (!(Test-Port $service.Port)) {
        Write-Host "  Port $($service.Port) is in use, attempting to free..." -ForegroundColor Yellow
        Stop-ProcessOnPort $service.Port
    }
}

Write-Host ""
Write-Host "Step 2: Building solution..." -ForegroundColor Cyan
dotnet build --configuration Release --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed! Please fix errors and try again." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Starting services..." -ForegroundColor Cyan
Write-Host ""

foreach ($service in $services) {
    Write-Host "  Starting $($service.Name) on port $($service.Port)..." -ForegroundColor $service.Color
    
    $windowTitle = "WMS - $($service.Name) [:$($service.Port)]"
    $command = "cd $($service.Path); `$host.UI.RawUI.WindowTitle = '$windowTitle'; dotnet run --urls=https://localhost:$($service.Port); Read-Host 'Press Enter to close'"
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
    
    # Small delay between starts
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All Services Started Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "  Gateway:   https://localhost:5000 (Unified Entry Point)" -ForegroundColor Magenta
Write-Host "  Auth:      https://localhost:5001" -ForegroundColor Green
Write-Host "  Products:  https://localhost:5002" -ForegroundColor Yellow
Write-Host "  Locations: https://localhost:5003" -ForegroundColor Blue
Write-Host "  Inbound:   https://localhost:5004" -ForegroundColor Cyan
Write-Host "  Outbound:  https://localhost:5005" -ForegroundColor DarkYellow
Write-Host "  Inventory: https://localhost:5006" -ForegroundColor DarkCyan
Write-Host "  Payment:   https://localhost:5007" -ForegroundColor DarkGreen
Write-Host "  Delivery:  https://localhost:5008" -ForegroundColor DarkMagenta
Write-Host ""
Write-Host "Recommended Usage:" -ForegroundColor Yellow
Write-Host "  1. Use Gateway for all requests: https://localhost:5000" -ForegroundColor White
Write-Host "  2. Gateway Routes:" -ForegroundColor White
Write-Host "     - /auth/*      -> Auth API" -ForegroundColor Gray
Write-Host "     - /products/*  -> Products API" -ForegroundColor Gray
Write-Host "     - /locations/* -> Locations API" -ForegroundColor Gray
Write-Host "     - /inventory/* -> Inventory API" -ForegroundColor Gray
Write-Host "     - /inbound/*   -> Inbound API" -ForegroundColor Gray
Write-Host "     - /outbound/*  -> Outbound API" -ForegroundColor Gray
Write-Host "     - /payment/*   -> Payment API" -ForegroundColor Gray
Write-Host "     - /delivery/*  -> Delivery API" -ForegroundColor Gray
Write-Host ""
Write-Host "Quick Test:" -ForegroundColor Yellow
Write-Host "  Gateway Health: https://localhost:5000/health" -ForegroundColor White
Write-Host "  Gateway Info:   https://localhost:5000/gateway/info" -ForegroundColor White
Write-Host ""
Write-Host "Default Credentials:" -ForegroundColor Yellow
Write-Host "  Username: admin" -ForegroundColor White
Write-Host "  Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Yellow
Write-Host "  See API_GATEWAY_GUIDE.md for complete usage guide" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop this script (services will continue running)" -ForegroundColor Gray
Write-Host "To stop services, close their individual PowerShell windows" -ForegroundColor Gray
Write-Host ""

# Keep script running
Write-Host "Monitoring services... (Press Ctrl+C to exit monitoring)" -ForegroundColor Cyan
while ($true) {
    Start-Sleep -Seconds 30
    
    # Optional: Check if services are still running
    $runningCount = 0
    foreach ($service in $services) {
        if (!(Test-Port $service.Port)) {
            $runningCount++
        }
    }
    
    if ($runningCount -eq 0) {
        Write-Host ""
        Write-Host "All services appear to be stopped." -ForegroundColor Yellow
        break
    }
}
