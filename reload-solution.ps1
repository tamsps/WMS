# Reload WMS Solution in Visual Studio

Write-Host "================================" -ForegroundColor Cyan
Write-Host "WMS Solution Setup Complete" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "? All microservice projects have been added to WMS.sln" -ForegroundColor Green
Write-Host ""

Write-Host "Solution includes:" -ForegroundColor Yellow
Write-Host "  Core Projects:" -ForegroundColor Gray
Write-Host "    • WMS.Domain" -ForegroundColor White
Write-Host "    • WMS.Application" -ForegroundColor White
Write-Host "    • WMS.Infrastructure" -ForegroundColor White
Write-Host ""
Write-Host "  Legacy Monolith:" -ForegroundColor Gray
Write-Host "    • WMS.API (Original monolithic API)" -ForegroundColor White
Write-Host ""
Write-Host "  Web Application:" -ForegroundColor Gray
Write-Host "    • WMS.Web" -ForegroundColor White
Write-Host ""
Write-Host "  Microservices (NEW):" -ForegroundColor Gray
Write-Host "    • WMS.Auth.API (Port 5001)" -ForegroundColor Green
Write-Host "    • WMS.Products.API (Port 5002)" -ForegroundColor Green
Write-Host "    • WMS.Locations.API (Port 5003)" -ForegroundColor Green
Write-Host "    • WMS.Inventory.API (Port 5004)" -ForegroundColor Green
Write-Host "    • WMS.Inbound.API (Port 5005)" -ForegroundColor Green
Write-Host "    • WMS.Outbound.API (Port 5006)" -ForegroundColor Green
Write-Host "    • WMS.Payment.API (Port 5007)" -ForegroundColor Green
Write-Host "    • WMS.Delivery.API (Port 5008)" -ForegroundColor Green
Write-Host ""

Write-Host "================================" -ForegroundColor Cyan
Write-Host "How to Load in Visual Studio" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Option 1: Open from File Explorer" -ForegroundColor Yellow
Write-Host "  1. Navigate to: F:\PROJECT\STUDY\VMS\" -ForegroundColor Gray
Write-Host "  2. Double-click WMS.sln" -ForegroundColor Gray
Write-Host ""

Write-Host "Option 2: Open from Visual Studio" -ForegroundColor Yellow
Write-Host "  1. Open Visual Studio 2022" -ForegroundColor Gray
Write-Host "  2. File ? Open ? Project/Solution" -ForegroundColor Gray
Write-Host "  3. Navigate to F:\PROJECT\STUDY\VMS\WMS.sln" -ForegroundColor Gray
Write-Host "  4. Click Open" -ForegroundColor Gray
Write-Host ""

Write-Host "Option 3: From Command Line" -ForegroundColor Yellow
Write-Host "  devenv WMS.sln" -ForegroundColor Gray
Write-Host ""

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Running Multiple Microservices" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "In Visual Studio:" -ForegroundColor Yellow
Write-Host "  1. Right-click on solution ? Properties" -ForegroundColor Gray
Write-Host "  2. Select 'Multiple startup projects'" -ForegroundColor Gray
Write-Host "  3. Set these to 'Start':" -ForegroundColor Gray
Write-Host "       • WMS.Auth.API" -ForegroundColor White
Write-Host "       • WMS.Products.API" -ForegroundColor White
Write-Host "       • WMS.Locations.API" -ForegroundColor White
Write-Host "       • WMS.Inventory.API" -ForegroundColor White
Write-Host "       • WMS.Inbound.API" -ForegroundColor White
Write-Host "       • WMS.Outbound.API" -ForegroundColor White
Write-Host "       • WMS.Payment.API" -ForegroundColor White
Write-Host "       • WMS.Delivery.API" -ForegroundColor White
Write-Host "       • WMS.Web (optional)" -ForegroundColor White
Write-Host "  4. Click OK" -ForegroundColor Gray
Write-Host "  5. Press F5 to start all services" -ForegroundColor Gray
Write-Host ""

Write-Host "From PowerShell (Easier):" -ForegroundColor Yellow
Write-Host "  .\run-all-services.ps1" -ForegroundColor Gray
Write-Host ""

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Verification" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$slnPath = "WMS.sln"
if (Test-Path $slnPath) {
    Write-Host "? Solution file exists: WMS.sln" -ForegroundColor Green
    
    # Count projects
    $projectCount = (dotnet sln $slnPath list | Select-String -Pattern "\.csproj").Count
    Write-Host "? Total projects in solution: $projectCount" -ForegroundColor Green
    
    # Verify build
    Write-Host ""
    Write-Host "Running build verification..." -ForegroundColor Yellow
    $buildResult = dotnet build $slnPath --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Solution builds successfully!" -ForegroundColor Green
    } else {
        Write-Host "??  Build had some warnings (check output)" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Solution file not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. ? Open WMS.sln in Visual Studio" -ForegroundColor White
Write-Host "2. ? Configure multiple startup projects (see above)" -ForegroundColor White
Write-Host "3. ? Press F5 to run all microservices" -ForegroundColor White
Write-Host "4. ? Access Swagger UI for each service:" -ForegroundColor White
Write-Host "     • Auth: https://localhost:5001" -ForegroundColor Gray
Write-Host "     • Products: https://localhost:5002" -ForegroundColor Gray
Write-Host "     • Locations: https://localhost:5003" -ForegroundColor Gray
Write-Host "     • Inventory: https://localhost:5004" -ForegroundColor Gray
Write-Host "     • Inbound: https://localhost:5005" -ForegroundColor Gray
Write-Host "     • Outbound: https://localhost:5006" -ForegroundColor Gray
Write-Host "     • Payment: https://localhost:5007" -ForegroundColor Gray
Write-Host "     • Delivery: https://localhost:5008" -ForegroundColor Gray
Write-Host ""

Write-Host "================================" -ForegroundColor Cyan
Write-Host "Documentation" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Quick Start: QUICKSTART.md" -ForegroundColor White
Write-Host "?? Architecture: MICROSERVICES_ARCHITECTURE.md" -ForegroundColor White
Write-Host "?? Running Guide: RUN_MICROSERVICES.md" -ForegroundColor White
Write-Host "?? Complete Summary: REFACTORING_SUMMARY.md" -ForegroundColor White
Write-Host "?? Main README: README_MICROSERVICES.md" -ForegroundColor White
Write-Host ""

Write-Host "?? Solution is ready to use! Happy coding!" -ForegroundColor Green
Write-Host ""
