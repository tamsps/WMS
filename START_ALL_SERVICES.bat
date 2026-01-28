@echo off
REM ============================================================================
REM WMS (Warehouse Management System) - Start All Services
REM ============================================================================
REM This script starts all WMS services in separate command windows
REM Each service runs on a unique port for independent deployment
REM
REM Services Started:
REM   - WMS.API (Port 5011)
REM   - WMS.Auth.API (Port 5002)
REM   - WMS.Products.API (Port 5003)
REM   - WMS.Locations.API (Port 5004)
REM   - WMS.Inbound.API (Port 5005)
REM   - WMS.Outbound.API (Port 5006)
REM   - WMS.Payment.API (Port 5007)
REM   - WMS.Delivery.API (Port 5009)
REM   - WMS.Inventory.API (Port 5010)
REM   - WMS.Gateway (Port 5000)
REM   - WMS.Web (Port 5001)
REM
REM Requirements:
REM   - .NET 9 SDK installed
REM   - SQL Server LocalDB with WMSDB database
REM   - All ports available (5000-5011)
REM   - Administrator privileges (optional, for process management)
REM ============================================================================

setlocal enabledelayedexpansion

REM Set colors and styling
color 0A
title WMS - Starting All Services

REM Set project root directory - ADJUST THIS PATH IF NEEDED
set PROJECT_ROOT=F:\PROJECT\STUDY\VMS

REM Verify project root exists
if not exist "%PROJECT_ROOT%" (
    echo ERROR: Project root directory not found: %PROJECT_ROOT%
    echo Please update PROJECT_ROOT in this script
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo          WMS - WAREHOUSE MANAGEMENT SYSTEM - SERVICE STARTUP
echo ============================================================================
echo.
echo Project Root: %PROJECT_ROOT%
echo Starting all services in separate windows...
echo.
echo SERVICES TO START:
echo   [1/11] WMS.API (Port 5011) - Main API Backend
echo   [2/11] WMS.Auth.API (Port 5002) - Authentication Service
echo   [3/11] WMS.Products.API (Port 5003) - Product Management
echo   [4/11] WMS.Locations.API (Port 5004) - Location Management
echo   [5/11] WMS.Inbound.API (Port 5005) - Inbound Operations
echo   [6/11] WMS.Outbound.API (Port 5006) - Outbound Operations
echo   [7/11] WMS.Payment.API (Port 5007) - Payment Processing
echo   [8/11] WMS.Delivery.API (Port 5009) - Delivery Tracking
echo   [9/11] WMS.Inventory.API (Port 5010) - Inventory Management
echo  [10/11] WMS.Gateway (Port 5000) - API Gateway
echo  [11/11] WMS.Web (Port 5001) - Web User Interface
echo.
echo ============================================================================
echo.

REM Start WMS.API
echo [1/11] Starting WMS.API (Port 5011)...
start "WMS.API - Port 5011" /d "%PROJECT_ROOT%\WMS.API" cmd /k "dotnet run --urls https://localhost:5011"
timeout /t 2 /nobreak

REM Start WMS.Auth.API
echo [2/11] Starting WMS.Auth.API (Port 5002)...
start "WMS.Auth.API - Port 5002" /d "%PROJECT_ROOT%\WMS.Auth.API" cmd /k "dotnet run --urls https://localhost:5002"
timeout /t 2 /nobreak

REM Start WMS.Products.API
echo [3/11] Starting WMS.Products.API (Port 5003)...
start "WMS.Products.API - Port 5003" /d "%PROJECT_ROOT%\WMS.Products.API" cmd /k "dotnet run --urls https://localhost:5003"
timeout /t 2 /nobreak

REM Start WMS.Locations.API
echo [4/11] Starting WMS.Locations.API (Port 5004)...
start "WMS.Locations.API - Port 5004" /d "%PROJECT_ROOT%\WMS.Locations.API" cmd /k "dotnet run --urls https://localhost:5004"
timeout /t 2 /nobreak

REM Start WMS.Inbound.API
echo [5/11] Starting WMS.Inbound.API (Port 5005)...
start "WMS.Inbound.API - Port 5005" /d "%PROJECT_ROOT%\WMS.Inbound.API" cmd /k "dotnet run --urls https://localhost:5005"
timeout /t 2 /nobreak

REM Start WMS.Outbound.API
echo [6/11] Starting WMS.Outbound.API (Port 5006)...
start "WMS.Outbound.API - Port 5006" /d "%PROJECT_ROOT%\WMS.Outbound.API" cmd /k "dotnet run --urls https://localhost:5006"
timeout /t 2 /nobreak

REM Start WMS.Payment.API
echo [7/11] Starting WMS.Payment.API (Port 5007)...
start "WMS.Payment.API - Port 5007" /d "%PROJECT_ROOT%\WMS.Payment.API" cmd /k "dotnet run --urls https://localhost:5007"
timeout /t 2 /nobreak

REM Start WMS.Delivery.API
echo [8/11] Starting WMS.Delivery.API (Port 5009)...
start "WMS.Delivery.API - Port 5009" /d "%PROJECT_ROOT%\WMS.Delivery.API" cmd /k "dotnet run --urls https://localhost:5009"
timeout /t 2 /nobreak

REM Start WMS.Inventory.API
echo [9/11] Starting WMS.Inventory.API (Port 5010)...
start "WMS.Inventory.API - Port 5010" /d "%PROJECT_ROOT%\WMS.Inventory.API" cmd /k "dotnet run --urls https://localhost:5010"
timeout /t 2 /nobreak

REM Start WMS.Gateway
echo [10/11] Starting WMS.Gateway (Port 5000)...
start "WMS.Gateway - Port 5000" /d "%PROJECT_ROOT%\WMS.Gateway" cmd /k "dotnet run --urls https://localhost:5000"
timeout /t 3 /nobreak

REM Start WMS.Web
echo [11/11] Starting WMS.Web (Port 5001)...
start "WMS.Web - Port 5001" /d "%PROJECT_ROOT%\WMS.Web" cmd /k "dotnet run --urls https://localhost:5001"
timeout /t 2 /nobreak

echo.
echo ============================================================================
echo                     ALL SERVICES ARE STARTING...
echo ============================================================================
echo.
echo STARTUP NOTES:
echo   - Each service opens in a separate command window
echo   - Services may take 5-10 seconds to fully initialize (first run slower)
echo   - Console output shows service startup logs
echo   - Services will be ready when you see "Application started. Press Ctrl+C to shut down"
echo.
echo ACCESS POINTS:
echo   - Web UI:              https://localhost:5001
echo   - API Gateway:         https://localhost:5000
echo   - Swagger/OpenAPI:     https://localhost:5000/swagger
echo   - Swagger JSON:        https://localhost:5000/swagger/v1/swagger.json
echo.
echo USEFUL LINKS:
echo   - Health Check:        https://localhost:5000/health
echo   - Gateway Status:      https://localhost:5000/health/ready
echo.
echo LOGIN CREDENTIALS (Default):
echo   - Username: admin
echo   - Password: Admin@123
echo.
echo   OR
echo.
echo   - Username: user
echo   - Password: User@123
echo.
echo ============================================================================
echo.
echo TROUBLESHOOTING:
echo   - If a service fails to start, check the port is available
echo   - Use STOP_ALL_SERVICES.bat to stop all services
echo   - Use HEALTH_CHECK.bat to verify all services are running
echo.
echo Close this window when ready, or press any key to continue...
pause

endlocal
