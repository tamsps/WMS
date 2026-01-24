# Fix Build Errors - Stop Running Processes

## ?? Problem

**Error**: Build fails with file locking errors

```
error MSB3021: Unable to copy file because it is being used by another process.
The file is locked by: "WMS.Gateway (2324)" and "WMS.Web (42904)"
```

## ? Solution

### Option 1: Stop Processes Manually (Recommended)

**In Visual Studio**:
1. Press `Ctrl+Shift+F5` or click **Stop Debugging** (red square button)
2. Or close the terminal/console windows running the apps
3. Then rebuild

**In PowerShell/Terminal**:
```powershell
# Stop WMS.Gateway
Stop-Process -Name "WMS.Gateway" -Force

# Stop WMS.Web
Stop-Process -Name "WMS.Web" -Force

# Or stop all dotnet processes
Get-Process dotnet | Stop-Process -Force
```

### Option 2: Use This Script

Save as `stop-all-services.ps1`:

```powershell
# Stop All WMS Services
Write-Host "Stopping all WMS services..." -ForegroundColor Cyan

$processes = @(
    "WMS.Gateway",
    "WMS.Web",
    "WMS.Auth.API",
    "WMS.Products.API",
    "WMS.Locations.API",
    "WMS.Inventory.API",
    "WMS.Inbound.API",
    "WMS.Outbound.API",
    "WMS.Payment.API",
    "WMS.Delivery.API"
)

foreach ($proc in $processes) {
    $running = Get-Process -Name $proc -ErrorAction SilentlyContinue
    if ($running) {
        Stop-Process -Name $proc -Force
        Write-Host "? Stopped $proc" -ForegroundColor Green
    }
}

# Also stop any dotnet processes
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "? All services stopped!" -ForegroundColor Green
Write-Host "You can now build the solution." -ForegroundColor Yellow
```

**Run it**:
```powershell
.\stop-all-services.ps1
```

---

## ?? Then Build

```powershell
# Clean solution
dotnet clean

# Rebuild
dotnet build
```

Or in Visual Studio: `Ctrl+Shift+B`

---

## ? Complete Workflow

### 1. Stop Running Services

```powershell
.\stop-all-services.ps1
```

### 2. Clean Build

```powershell
dotnet clean
```

### 3. Build Solution

```powershell
dotnet build
```

### 4. Start Services Again

```powershell
.\run-all-services.ps1
```

---

## ?? Quick Reference

| Task | Command | Shortcut |
|------|---------|----------|
| **Stop Debugging** | Menu ? Debug ? Stop | `Shift+F5` |
| **Stop All Processes** | `.\stop-all-services.ps1` | - |
| **Clean Solution** | `dotnet clean` | - |
| **Build Solution** | `dotnet build` | `Ctrl+Shift+B` |
| **Rebuild Solution** | `dotnet clean && dotnet build` | `Ctrl+Alt+F7` |

---

## ?? Prevention Tips

### Always Stop Before Building

```powershell
# Stop ? Clean ? Build ? Run workflow
.\stop-all-services.ps1
dotnet clean
dotnet build
.\run-all-services.ps1
```

### Use Visual Studio Hot Reload

Instead of stopping and rebuilding:
- Make code changes while running
- Visual Studio will apply changes automatically (Hot Reload)
- Only rebuild when adding new files or major changes

---

## ?? Currently Running Processes

Based on the error:

| Process | PID | Status |
|---------|-----|--------|
| **WMS.Gateway** | 2324 | ? Running (blocking build) |
| **WMS.Web** | 42904 | ? Running (blocking build) |

**You must stop these before building!**

---

## ?? Summary

**Problem**: Running processes lock DLL files  
**Solution**: Stop all processes before building  
**Quick Fix**: `.\stop-all-services.ps1` then `dotnet build`

**This is NOT a code error - just file locking! ?**
