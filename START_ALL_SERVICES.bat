@echo off
REM ============================================================================
REM WMS (Warehouse Management System) - Start All Services
REM ============================================================================
REM This script starts all WMS services in separate command windows
REM Each service runs on a unique port for independent deployment
REM
REM Services Started:
REM   - WMS.Auth.API (Port 5190)
REM   - WMS.Products.API (Port 62527)
REM   - WMS.Locations.API (Port 62522)
REM   - WMS.Inbound.API (Port 62520)
REM   - WMS.Outbound.API (Port 62519)
REM   - WMS.Payment.API (Port 62521)
REM   - WMS.Delivery.API (Port 62529)
REM   - WMS.Inventory.API (Port 62531)
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
echo   [1/10] WMS.Auth.API (Port 5190) - Authentication Service
echo   [2/10] WMS.Products.API (Port 62527) - Product Management
echo   [3/10] WMS.Locations.API (Port 62522) - Location Management
echo   [4/10] WMS.Inbound.API (Port 62520) - Inbound Operations
echo   [5/10] WMS.Outbound.API (Port 62519) - Outbound Operations
echo   [6/10] WMS.Payment.API (Port 62521) - Payment Processing
echo   [7/10] WMS.Delivery.API (Port 62529) - Delivery Tracking
echo   [8/10] WMS.Inventory.API (Port 62531) - Inventory Management
echo   [9/10] WMS.Gateway (Port 5000) - API Gateway
echo  [10/10] WMS.Web (Port 5223) - Web User Interface
echo.
echo ============================================================================
echo.

REM Start WMS.Auth.API
echo [1/10] Starting WMS.Auth.API (Port 5190)...
start "WMS.Auth.API - Port 5190" /d "%PROJECT_ROOT%\WMS.Auth.API" cmd /k "dotnet run --urls http://localhost:5190"
timeout /t 2 /nobreak

REM Start WMS.Locations.API
echo [3/10] Starting WMS.Locations.API (Port 62522)...
start "WMS.Locations.API - Port 62522" /d "%PROJECT_ROOT%\WMS.Locations.API" cmd /k "dotnet run --urls http://localhost:62522"
timeout /t 2 /nobreak

REM Start WMS.Inbound.API
echo [4/10] Starting WMS.Inbound.API (Port 62520)...
start "WMS.Inbound.API - Port 62520" /d "%PROJECT_ROOT%\WMS.Inbound.API" cmd /k "dotnet run --urls http://localhost:62520"
timeout /t 2 /nobreak

REM Start WMS.Outbound.API
echo [5/10] Starting WMS.Outbound.API (Port 62519)...
start "WMS.Outbound.API - Port 62519" /d "%PROJECT_ROOT%\WMS.Outbound.API" cmd /k "dotnet run --urls http://localhost:62519"
timeout /t 2 /nobreak

REM Start WMS.Payment.API
echo [6/10] Starting WMS.Payment.API (Port 62521)...
start "WMS.Payment.API - Port 62521" /d "%PROJECT_ROOT%\WMS.Payment.API" cmd /k "dotnet run --urls http://localhost:62521"
timeout /t 2 /nobreak

REM Start WMS.Delivery.API
echo [7/10] Starting WMS.Delivery.API (Port 62529)...
start "WMS.Delivery.API - Port 62529" /d "%PROJECT_ROOT%\WMS.Delivery.API" cmd /k "dotnet run --urls http://localhost:62529"
timeout /t 2 /nobreak

REM Start WMS.Inventory.API
echo [8/10] Starting WMS.Inventory.API (Port 62531)...
start "WMS.Inventory.API - Port 62531" /d "%PROJECT_ROOT%\WMS.Inventory.API" cmd /k "dotnet run --urls http://localhost:62531"
timeout /t 2 /nobreak

REM Start WMS.Products.API
echo [2/10] Starting WMS.Products.API (Port 62527)...
start "WMS.Products.API - Port 62527" /d "%PROJECT_ROOT%\WMS.Products.API" cmd /k "dotnet run --urls http://localhost:62527"
timeout /t 2 /nobreak

REM Start WMS.Gateway
echo [9/10] Starting WMS.Gateway (Port 5000)...
start "WMS.Gateway - Port 5000" /d "%PROJECT_ROOT%\WMS.Gateway" cmd /k "dotnet run --urls http://localhost:5000"
timeout /t 3 /nobreak



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
echo   - Web UI:              http://localhost:5223
echo   - API Gateway:         http://localhost:5000
echo   - Swagger/OpenAPI:     http://localhost:5000/swagger
echo   - Swagger JSON:        http://localhost:5000/swagger/v1/swagger.json
echo.
echo USEFUL LINKS:
echo   - Health Check:        http://localhost:5000/health
echo   - Gateway Status:      http://localhost:5000/health/ready
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
