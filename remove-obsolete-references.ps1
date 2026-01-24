# Remove WMS.Application and WMS.Infrastructure References
# These projects are no longer used after CQRS refactoring

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   Removing Obsolete Project References              ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$apiProjects = @(
    "WMS.Auth.API\WMS.Auth.API.csproj",
    "WMS.Products.API\WMS.Products.API.csproj",
    "WMS.Locations.API\WMS.Locations.API.csproj",
    "WMS.Inventory.API\WMS.Inventory.API.csproj",
    "WMS.Inbound.API\WMS.Inbound.API.csproj",
    "WMS.Outbound.API\WMS.Outbound.API.csproj",
    "WMS.Payment.API\WMS.Payment.API.csproj",
    "WMS.Delivery.API\WMS.Delivery.API.csproj",
    "WMS.Web\WMS.Web.csproj"
)

$removedCount = 0
$skippedCount = 0

foreach ($project in $apiProjects) {
    if (Test-Path $project) {
        Write-Host "Processing: $project" -ForegroundColor Cyan
        
        try {
            # Remove WMS.Application reference
            dotnet remove $project reference "..\WMS.Application\WMS.Application.csproj" 2>&1 | Out-Null
            
            # Remove WMS.Infrastructure reference
            dotnet remove $project reference "..\WMS.Infrastructure\WMS.Infrastructure.csproj" 2>&1 | Out-Null
            
            Write-Host "? Removed obsolete references from $($project.Split('\')[0])" -ForegroundColor Green
            $removedCount++
        }
        catch {
            Write-Host "? $project - references may not exist" -ForegroundColor Yellow
            $skippedCount++
        }
    }
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?   Complete!                                         ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  ? Updated: $removedCount projects" -ForegroundColor Green
Write-Host "  ? Skipped: $skippedCount projects" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  dotnet clean" -ForegroundColor White
Write-Host "  dotnet build" -ForegroundColor White
Write-Host ""
