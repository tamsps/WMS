# Update JWT Configuration in All API Projects
# This script ensures all microservices have the same JWT configuration

param(
    [Parameter(Mandatory=$false)]
    [string]$SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    
    [Parameter(Mandatory=$false)]
    [string]$Issuer = "WMS.Auth.API",
    
    [Parameter(Mandatory=$false)]
    [string]$Audience = "WMS.Client"
)

$ErrorActionPreference = "Stop"

Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   WMS JWT Configuration Updater                     ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$apis = @(
    "WMS.Auth.API",
    "WMS.Products.API",
    "WMS.Locations.API",
    "WMS.Inventory.API",
    "WMS.Inbound.API",
    "WMS.Outbound.API",
    "WMS.Payment.API",
    "WMS.Delivery.API"
)

$updatedCount = 0
$skippedCount = 0
$failedCount = 0

foreach ($api in $apis) {
    $appsettingsPath = Join-Path $api "appsettings.json"
    
    if (-not (Test-Path $appsettingsPath)) {
        Write-Host "? Skipped: $api (appsettings.json not found)" -ForegroundColor Yellow
        $skippedCount++
        continue
    }
    
    try {
        Write-Host "Updating $api..." -ForegroundColor Cyan
        
        # Read current appsettings
        $content = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        # Add or update JwtSettings
        $jwtSettings = @{
            SecretKey = $SecretKey
            Issuer = $Issuer
            Audience = $Audience
            ExpirationMinutes = 60
            RefreshTokenExpirationDays = 7
        }
        
        if ($content.PSObject.Properties.Name -contains "JwtSettings") {
            Write-Host "  - Updating existing JwtSettings" -ForegroundColor Gray
            $content.JwtSettings = $jwtSettings
        } else {
            Write-Host "  - Adding new JwtSettings" -ForegroundColor Gray
            $content | Add-Member -Type NoteProperty -Name "JwtSettings" -Value $jwtSettings -Force
        }
        
        # Add or update Cors settings
        $corsSettings = @{
            AllowedOrigins = @(
                "http://localhost:5000",
                "https://localhost:5001",
                "https://localhost:7000",
                "http://localhost:3000"
            )
        }
        
        if ($content.PSObject.Properties.Name -contains "Cors") {
            Write-Host "  - Updating existing Cors" -ForegroundColor Gray
            $content.Cors = $corsSettings
        } else {
            Write-Host "  - Adding new Cors" -ForegroundColor Gray
            $content | Add-Member -Type NoteProperty -Name "Cors" -Value $corsSettings -Force
        }
        
        # Write back to file
        $content | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        
        Write-Host "? $api updated successfully" -ForegroundColor Green
        $updatedCount++
    }
    catch {
        Write-Host "? Failed to update $api : $($_.Exception.Message)" -ForegroundColor Red
        $failedCount++
    }
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  Update Complete!                                   ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  ? Updated: $updatedCount APIs" -ForegroundColor Green
Write-Host "  ? Skipped: $skippedCount APIs" -ForegroundColor Yellow
Write-Host "  ? Failed: $failedCount APIs" -ForegroundColor Red
Write-Host ""

if ($updatedCount -gt 0) {
    Write-Host "JWT Configuration:" -ForegroundColor Cyan
    Write-Host "  SecretKey: $($SecretKey.Substring(0, 20))..." -ForegroundColor White
    Write-Host "  Issuer: $Issuer" -ForegroundColor White
    Write-Host "  Audience: $Audience" -ForegroundColor White
    Write-Host "  AccessToken Expiration: 60 minutes" -ForegroundColor White
    Write-Host "  RefreshToken Expiration: 7 days" -ForegroundColor White
    Write-Host ""
}

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review updated appsettings.json files" -ForegroundColor White
Write-Host "  2. Test Auth.API: cd WMS.Auth.API && dotnet run" -ForegroundColor White
Write-Host "  3. Test login: POST https://localhost:7081/api/auth/login" -ForegroundColor White
Write-Host ""

# Warn about production
Write-Host "??  IMPORTANT: Change SecretKey before deploying to production!" -ForegroundColor Red
Write-Host "   Generate secure key: [System.Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))" -ForegroundColor Yellow
Write-Host ""
