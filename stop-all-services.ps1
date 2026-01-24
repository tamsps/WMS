# Stop All WMS Services
# Use this script to stop all running WMS services before building

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   Stopping All WMS Services                         ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$processes = @(
    "WMS.Gateway",
    "WMS.Web",
    "WMS.Auth.API",
    "WMS.Products.API",
    "WMS.Locations.API",
    "WMS.Inventory.API",
    "WMS.Inbound.API",
    "WMS.Outbound.API",
    "WMS.Payment.API",
    "WMS.Delivery.API"
)

$stoppedCount = 0
$notRunningCount = 0

foreach ($procName in $processes) {
    $running = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($running) {
        try {
            Stop-Process -Name $procName -Force -ErrorAction Stop
            Write-Host "? Stopped: $procName" -ForegroundColor Green
            $stoppedCount++
        }
        catch {
            Write-Host "? Failed to stop: $procName" -ForegroundColor Red
        }
    }
    else {
        Write-Host "- Not running: $procName" -ForegroundColor Gray
        $notRunningCount++
    }
}

# Also stop any orphaned dotnet processes
Write-Host ""
Write-Host "Checking for orphaned dotnet processes..." -ForegroundColor Yellow

$dotnetProcesses = Get-Process dotnet -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    $dotnetProcesses | Stop-Process -Force
    Write-Host "? Stopped $($dotnetProcesses.Count) dotnet process(es)" -ForegroundColor Green
}
else {
    Write-Host "- No orphaned dotnet processes found" -ForegroundColor Gray
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?   Complete!                                         ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  ? Stopped: $stoppedCount services" -ForegroundColor Green
Write-Host "  - Not running: $notRunningCount services" -ForegroundColor Gray
Write-Host ""
Write-Host "You can now build the solution:" -ForegroundColor Cyan
Write-Host "  dotnet clean" -ForegroundColor White
Write-Host "  dotnet build" -ForegroundColor White
Write-Host ""
Write-Host "Or in Visual Studio: Ctrl+Shift+B" -ForegroundColor Yellow
Write-Host ""
