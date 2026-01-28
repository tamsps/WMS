@echo off
REM ============================================================================
REM WMS (Warehouse Management System) - Database Setup Script
REM ============================================================================
REM This script sets up the WMSDB database with all migrations
REM Handles database creation and schema initialization
REM
REM Prerequisites:
REM   - SQL Server LocalDB installed
REM   - .NET 9 SDK installed
REM   - Administrator privileges recommended
REM ============================================================================

setlocal enabledelayedexpansion

color 0B
title WMS Database Setup

REM Set project root directory - ADJUST IF NEEDED
set PROJECT_ROOT=F:\PROJECT\STUDY\VMS

echo.
echo ============================================================================
echo          WMS - DATABASE SETUP AND MIGRATIONS
echo ============================================================================
echo.
echo Database: WMSDB
echo Server: (localdb)\mssqllocaldb
echo Project Root: %PROJECT_ROOT%
echo.

REM Verify project root exists
if not exist "%PROJECT_ROOT%" (
    echo ERROR: Project root not found: %PROJECT_ROOT%
    echo Please update PROJECT_ROOT in this script
    pause
    exit /b 1
)

REM Verify solution file exists
if not exist "%PROJECT_ROOT%\WMS.sln" (
    echo ERROR: WMS.sln not found in %PROJECT_ROOT%
    pause
    exit /b 1
)

echo ============================================================================
echo.

REM Step 1: Build Solution
echo STEP 1: Building Solution...
echo ============================================================================
cd /d "%PROJECT_ROOT%"

dotnet build WMS.sln -c Debug

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Solution build failed!
    echo Please fix compilation errors and try again
    pause
    exit /b 1
)

echo Build completed successfully
echo.

REM Step 2: Create/Update Database
echo STEP 2: Creating/Updating Database...
echo ============================================================================

echo Applying Entity Framework migrations...
echo.

REM Use WMS.Infrastructure as migrations assembly
dotnet ef database update ^
  --project "%PROJECT_ROOT%\WMS.Infrastructure" ^
  --startup-project "%PROJECT_ROOT%\WMS.API" ^
  --configuration Debug ^
  --verbose

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Database migration failed!
    echo Common causes:
    echo   - SQL Server LocalDB not installed
    echo   - Database already exists with schema conflicts
    echo   - Connection string incorrect
    echo.
    echo Try:
    echo   - Deleting existing WMSDB database
    echo   - Running SQL Server LocalDB: sqllocaldb start mssqllocaldb
    echo   - Check appsettings.json connection string
    pause
    exit /b 1
)

echo.
echo Database migration completed successfully
echo.

REM Step 3: Verify Database
echo STEP 3: Verifying Database...
echo ============================================================================

REM Check if database exists
for /f %%i in ('sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = 'WMSDB'" 2^>nul ^| findstr /R "[0-9]"') do set DB_EXISTS=%%i

if "%DB_EXISTS%"=="1" (
    echo Database WMSDB exists: [OK]
) else (
    echo WARNING: Database WMSDB not found
)

REM Count tables
for /f %%i in ('sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" 2^>nul ^| findstr /R "[0-9]"') do set TABLE_COUNT=%%i

echo Number of tables: %TABLE_COUNT%
echo.

REM Step 4: List Tables
echo STEP 4: Database Schema Tables...
echo ============================================================================
echo.

sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME" 2>nul

echo.

REM Step 5: Seed Data Check
echo STEP 5: Checking Seed Data...
echo ============================================================================
echo.

REM Check users
for /f %%i in ('sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT COUNT(*) FROM dbo.Users" 2^>nul ^| findstr /R "[0-9]"') do set USER_COUNT=%%i

REM Check products
for /f %%i in ('sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT COUNT(*) FROM dbo.Products" 2^>nul ^| findstr /R "[0-9]"') do set PRODUCT_COUNT=%%i

REM Check locations
for /f %%i in ('sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT COUNT(*) FROM dbo.Locations" 2^>nul ^| findstr /R "[0-9]"') do set LOCATION_COUNT=%%i

echo Users in database: %USER_COUNT%
echo Products in database: %PRODUCT_COUNT%
echo Locations in database: %LOCATION_COUNT%
echo.

REM Step 6: Summary
echo ============================================================================
echo                    DATABASE SETUP COMPLETED
echo ============================================================================
echo.
echo SUMMARY:
echo   - Solution built successfully
echo   - Database: WMSDB created/updated
echo   - Migrations applied
echo   - Database verified
echo.
echo DATABASE INFO:
echo   - Server: (localdb)\mssqllocaldb
echo   - Database: WMSDB
echo   - Tables: %TABLE_COUNT%
echo   - Users: %USER_COUNT%
echo   - Products: %PRODUCT_COUNT%
echo   - Locations: %LOCATION_COUNT%
echo.
echo DEFAULT LOGIN CREDENTIALS:
echo   - Username: admin
echo   - Password: Admin@123
echo.
echo   OR
echo.
echo   - Username: user
echo   - Password: User@123
echo.
echo NEXT STEPS:
echo   1. Start all services: START_ALL_SERVICES.bat
echo   2. Access Web UI: https://localhost:5001
echo   3. Login with credentials above
echo.
echo For detailed documentation:
echo   - Read: ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md
echo.
echo ============================================================================
echo.

pause
endlocal
