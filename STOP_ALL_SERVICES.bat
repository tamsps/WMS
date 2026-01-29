@echo off
REM ============================================================================
REM WMS (Warehouse Management System) - Stop All Services
REM ============================================================================
REM This script stops all running WMS services gracefully
REM Requires Administrator privileges to function properly
REM
REM IMPORTANT: Run this as Administrator for best results
REM ============================================================================

setlocal enabledelayedexpansion

color 0C
title WMS - Stopping All Services

echo.
echo ============================================================================
echo          WMS - STOPPING ALL SERVICES
echo ============================================================================
echo.
echo WARNING: This script requires Administrator privileges to work properly
echo If services don't stop, manually close each command window
echo.

REM Check for admin privileges (optional, for better error messages)
net session >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo WARNING: Not running as Administrator
    echo Some services may not stop properly
    echo.
    echo To run as Administrator:
    echo   1. Right-click this .bat file
    echo   2. Select "Run as Administrator"
    echo.
)

echo Attempting to stop services...
echo.

REM Define services and ports (based on Gateway configuration)
setlocal enabledelayedexpansion
set "ports[0]=5190" & set "services[0]=WMS.Auth.API"
set "ports[1]=62527" & set "services[1]=WMS.Products.API"
set "ports[2]=62522" & set "services[2]=WMS.Locations.API"
set "ports[3]=62520" & set "services[3]=WMS.Inbound.API"
set "ports[4]=62519" & set "services[4]=WMS.Outbound.API"
set "ports[5]=62521" & set "services[5]=WMS.Payment.API"
set "ports[6]=62529" & set "services[6]=WMS.Delivery.API"
set "ports[7]=62531" & set "services[7]=WMS.Inventory.API"
set "ports[8]=5000" & set "services[8]=WMS.Gateway"


REM Stop each service
for /l %%i in (0,1,9) do (
    set port=!ports[%%i]!
    set service=!services[%%i]!
    
    echo Stopping !service! on port !port!...
    
    REM Find and kill process on the port
    for /f "tokens=5" %%a in ('netstat -aon 2^>nul ^| find ":!port!"') do (
        taskkill /pid %%a /f /t >nul 2>&1
        if !ERRORLEVEL! equ 0 (
            echo   [STOPPED] Process !service! terminated
        ) else (
            echo   [INFO] No process found or already stopped
        )
    )
)

echo.
echo ============================================================================
echo                    ALL SERVICES STOPPED
echo ============================================================================
echo.
echo Services Status:
echo   - All dotnet processes should be terminated
echo   - Ports 5000, 5001, 5190, 62519-62531 should be available for next startup
echo.
echo To restart services:
echo   Run START_ALL_SERVICES.bat
echo.
echo To check if services are really stopped:
echo   netstat -ano | findstr :5000
echo   netstat -ano | findstr :5001
echo   etc...
echo.

pause
endlocal
