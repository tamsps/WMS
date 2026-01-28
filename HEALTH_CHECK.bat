@echo off
REM ============================================================================
REM WMS (Warehouse Management System) - Health Check Script
REM ============================================================================
REM This script checks the health status of all running services
REM Uses HTTPS with self-signed certificate bypass for local testing
REM ============================================================================

setlocal enabledelayedexpansion

color 0F
title WMS Health Check

echo.
echo ============================================================================
echo          WMS - SERVICE HEALTH CHECK
echo ============================================================================
echo.
echo Checking health endpoints of all WMS services...
echo.
echo Note: This requires 'curl' command (installed with recent Windows versions)
echo       or Git Bash installed
echo.

REM Define services and ports
set "services[0]=WMS.API::5011"
set "services[1]=WMS.Auth.API::5002"
set "services[2]=WMS.Products.API::5003"
set "services[3]=WMS.Locations.API::5004"
set "services[4]=WMS.Inbound.API::5005"
set "services[5]=WMS.Outbound.API::5006"
set "services[6]=WMS.Payment.API::5007"
set "services[7]=WMS.Delivery.API::5009"
set "services[8]=WMS.Inventory.API::5010"
set "services[9]=WMS.Gateway::5000"
set "services[10]=WMS.Web::5001"

REM Check if curl is available
where curl >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo WARNING: 'curl' command not found
    echo Installing curl or Git Bash is recommended
    echo.
    echo Alternative method: Use Postman or browser to check:
    echo   - https://localhost:5000/health
    echo   - https://localhost:5001/health
    echo.
)

echo ============================================================================
echo Service Health Status:
echo ============================================================================
echo.

set /a "count=0"
set /a "online=0"
set /a "offline=0"

for /l %%i in (0,1,10) do (
    for /f "tokens=1,2 delims=::" %%a in ("!services[%%i]!") do (
        set name=%%a
        set port=%%b
        set /a "count=!count!+1"
        
        echo Checking !name! (Port !port!)...
        
        REM Try to reach health endpoint
        curl -s -k https://localhost:!port!/health >nul 2>&1
        
        if !ERRORLEVEL! equ 0 (
            echo   Status: [ONLINE] ✓
            set /a "online=!online!+1"
        ) else (
            echo   Status: [OFFLINE] ✗
            set /a "offline=!offline!+1"
        )
    )
)

echo.
echo ============================================================================
echo Health Check Summary:
echo ============================================================================
echo.
echo Total Services: %count%
echo Online:        %online%
echo Offline:       %offline%
echo.

if %offline% equ 0 (
    echo Result: [ALL SERVICES RUNNING] ✓
) else (
    echo Result: [SOME SERVICES NOT RESPONDING]
    echo.
    echo Offline services:
    for /l %%i in (0,1,10) do (
        for /f "tokens=1,2 delims=::" %%a in ("!services[%%i]!") do (
            set name=%%a
            set port=%%b
            
            curl -s -k https://localhost:!port!/health >nul 2>&1
            if !ERRORLEVEL! neq 0 (
                echo   - !name! (Port !port!)
            )
        )
    )
    echo.
    echo Try starting services with: START_ALL_SERVICES.bat
)

echo.
echo ============================================================================
echo Access Points:
echo ============================================================================
echo.
echo Web UI:              https://localhost:5001
echo API Gateway:        https://localhost:5000
echo Swagger/OpenAPI:    https://localhost:5000/swagger
echo Gateway Health:     https://localhost:5000/health
echo.

echo ============================================================================
echo.

pause
endlocal
