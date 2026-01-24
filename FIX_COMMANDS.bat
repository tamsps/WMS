@echo off
echo Fixing all Program.cs files...
echo.

REM Note: This is a manual guide. Run these commands in PowerShell from the solution root directory.
echo Please run the following commands in PowerShell:
echo.
echo cd F:\PROJECT\STUDY\VMS
echo.
echo # Fix Inbound
echo (Get-Content "WMS.Inbound.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Inbound.API\Program.cs" -NoNewline
echo.
echo # Fix Outbound  
echo (Get-Content "WMS.Outbound.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Outbound.API\Program.cs" -NoNewline
echo.
echo # Fix Inventory
echo (Get-Content "WMS.Inventory.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Inventory.API\Program.cs" -NoNewline
echo.
echo # Fix Locations
echo (Get-Content "WMS.Locations.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Locations.API\Program.cs" -NoNewline
echo.
echo # Fix Products
echo (Get-Content "WMS.Products.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Products.API\Program.cs" -NoNewline
echo.
echo # Fix Payment
echo (Get-Content "WMS.Payment.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Payment.API\Program.cs" -NoNewline
echo.
echo # Fix Delivery
echo (Get-Content "WMS.Delivery.API\Program.cs" -Raw) -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");' ^| Set-Content "WMS.Delivery.API\Program.cs" -NoNewline
echo.
echo # Fix Auth (special case)
echo $content = Get-Content "WMS.Auth.API\Program.cs" -Raw; $content = $content -replace 'throw new InvalidOperationException\("JWT Secret[^\)]*', 'throw new InvalidOperationException("JWT SecretKey not configured");'; $content = $content -replace 'AddScoped^<IAuthService, AuthService^>', 'AddScoped^<WMS.Application.Interfaces.IAuthService, AuthService^>()'; $content = $content -replace 'AddScoped^<ITokenService^>\(', 'AddScoped^<WMS.Application.Interfaces.ITokenService^>('; Set-Content "WMS.Auth.API\Program.cs" -Value $content -NoNewline
echo.
echo Then run: dotnet build
echo.
pause
