# Fix All Program.cs Files - Clean Architecture Refactoring
# This script fixes the truncated exception messages in all microservice Program.cs files

Write-Host "Fixing Program.cs files for all microservices..." -ForegroundColor Cyan
Write-Host ""

$fixedCount = 0
$errorCount = 0

# Function to fix a Program.cs file
function Fix-ProgramFile {
    param(
        [string]$FilePath,
        [string]$ServiceName
    )
    
    try {
        Write-Host "Fixing $ServiceName..." -ForegroundColor Yellow
        
        $content = Get-Content -Path $FilePath -Raw
        
        # Fix the truncated InvalidOperationException line
        $content = $content -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");'
        
        # Ensure proper using statements
        if ($content -notmatch 'using WMS\.Domain\.Data;') {
            $content = $content -replace '(using WMS\.Infrastructure\.Data;)', 'using WMS.Domain.Data;'
        }
        
        if ($content -notmatch 'using WMS\.Domain\.Repositories;') {
            $content = $content -replace '(using WMS\.Infrastructure\.Repositories;)', 'using WMS.Domain.Repositories;'
        }
        
        # Save the fixed content
        Set-Content -Path $FilePath -Value $content -NoNewline
        
        Write-Host "  ? Fixed $ServiceName" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ? Error fixing $ServiceName : $_" -ForegroundColor Red
        return $false
    }
}

# Fix all microservice Program.cs files
$services = @(
    @{Path="WMS.Inbound.API\Program.cs"; Name="WMS.Inbound.API"},
    @{Path="WMS.Outbound.API\Program.cs"; Name="WMS.Outbound.API"},
    @{Path="WMS.Inventory.API\Program.cs"; Name="WMS.Inventory.API"},
    @{Path="WMS.Locations.API\Program.cs"; Name="WMS.Locations.API"},
    @{Path="WMS.Products.API\Program.cs"; Name="WMS.Products.API"},
    @{Path="WMS.Payment.API\Program.cs"; Name="WMS.Payment.API"},
    @{Path="WMS.Delivery.API\Program.cs"; Name="WMS.Delivery.API"}
)

foreach ($service in $services) {
    if (Test-Path $service.Path) {
        if (Fix-ProgramFile -FilePath $service.Path -ServiceName $service.Name) {
            $fixedCount++
        } else {
            $errorCount++
        }
    } else {
        Write-Host "  ? File not found: $($service.Path)" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "Fixing WMS.Auth.API (special case)..." -ForegroundColor Yellow

# Fix WMS.Auth.API separately (has different issues)
$authPath = "WMS.Auth.API\Program.cs"
if (Test-Path $authPath) {
    try {
        $content = Get-Content -Path $authPath -Raw
        
        # Fix the truncated exception
        $content = $content -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");'
        
        # Fix interface references
        $content = $content -replace 'builder\.Services\.AddScoped<IAuthService, AuthService>\(\);', 'builder.Services.AddScoped<WMS.Application.Interfaces.IAuthService, AuthService>();'
        $content = $content -replace 'builder\.Services\.AddScoped<ITokenService>\(', 'builder.Services.AddScoped<WMS.Application.Interfaces.ITokenService>('
        
        Set-Content -Path $authPath -Value $content -NoNewline
        Write-Host "  ? Fixed WMS.Auth.API" -ForegroundColor Green
        $fixedCount++
    }
    catch {
        Write-Host "  ? Error fixing WMS.Auth.API: $_" -ForegroundColor Red
        $errorCount++
    }
} else {
    Write-Host "  ? File not found: $authPath" -ForegroundColor Red
    $errorCount++
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Fixed: $fixedCount files" -ForegroundColor Green
Write-Host "  Errors: $errorCount files" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($errorCount -eq 0) {
    Write-Host "All files fixed successfully!" -ForegroundColor Green
    Write-Host "Run 'dotnet build' to verify the build succeeds." -ForegroundColor Yellow
} else {
    Write-Host "Some files had errors. Please check the output above." -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
