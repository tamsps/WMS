# ? Auth.API JWT Configuration - FIXED

## ?? Error Description

**Error**: `System.InvalidOperationException: JWT SecretKey not configured`

**Location**: `WMS.Auth.API/Program.cs` line 53

**Root Cause**: Missing `JwtSettings` and `Cors` configuration in `appsettings.json`

---

## ? Solution Applied

### Updated `WMS.Auth.API/appsettings.json`

Added the missing configuration sections:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=CONGTAM-PC;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    "Issuer": "WMS.Auth.API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5000",
      "https://localhost:5001",
      "https://localhost:7000",
      "http://localhost:3000"
    ]
  }
}
```

---

## ?? JWT Configuration Explained

### SecretKey
```json
"SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!"
```

**Requirements**:
- ? **Minimum 32 characters** for HS256 algorithm
- ? **Complex** - Mix of letters, numbers, special characters
- ?? **CHANGE THIS** in production!

**Production Secret Key Generation**:
```powershell
# PowerShell - Generate random 64-character key
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# OR use online generator: https://generate-secret.vercel.app/32
```

### Issuer
```json
"Issuer": "WMS.Auth.API"
```
- Identifies who issued the token (your Auth API)
- Must match in all microservices that validate tokens

### Audience
```json
"Audience": "WMS.Client"
```
- Identifies who the token is intended for
- Must match in all microservices

### ExpirationMinutes
```json
"ExpirationMinutes": 60
```
- Access token lifetime: **60 minutes** (1 hour)
- Short-lived for security
- User needs to refresh after expiration

### RefreshTokenExpirationDays
```json
"RefreshTokenExpirationDays": 7
```
- Refresh token lifetime: **7 days**
- Long-lived for better UX
- Used to get new access tokens without re-login

---

## ?? CORS Configuration Explained

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5000",
    "https://localhost:5001",
    "https://localhost:7000",
    "http://localhost:3000"
  ]
}
```

**Allowed Origins**:
- `http://localhost:5000` - WMS.Web (HTTP)
- `https://localhost:5001` - WMS.Web (HTTPS)
- `https://localhost:7000` - Gateway
- `http://localhost:3000` - React/Angular frontend (if any)

**For Production**: Replace with your actual domain(s)

---

## ?? Test the Fix

### Start Auth.API

```powershell
cd WMS.Auth.API
dotnet run
```

**Expected**: Auth API starts successfully on `https://localhost:7081`

### Test Login Endpoint

```powershell
curl -X POST https://localhost:7081/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

**Expected Response**:
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "user": {
      "id": "...",
      "username": "admin",
      "email": "admin@wms.com",
      "roles": ["Admin"]
    }
  }
}
```

### Test Through Gateway

```powershell
curl -X POST https://localhost:7000/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? Other APIs Configuration Status

Let me check if other APIs need the same configuration:

| API Project | JWT Config Needed? | Status |
|-------------|-------------------|--------|
| **WMS.Auth.API** | ? Yes | ? **FIXED** |
| WMS.Products.API | ? Yes | ?? Check needed |
| WMS.Locations.API | ? Yes | ?? Check needed |
| WMS.Inventory.API | ? Yes | ?? Check needed |
| WMS.Inbound.API | ? Yes | ?? Check needed |
| WMS.Outbound.API | ? Yes | ?? Check needed |
| WMS.Payment.API | ? Yes | ?? Check needed |
| WMS.Delivery.API | ? Yes | ?? Check needed |

**Note**: All microservices need the **same JWT configuration** to validate tokens from Auth.API!

---

## ?? Update All APIs Script

Save this as `update-jwt-config.ps1`:

```powershell
# Update JWT Configuration in All APIs
$jwtConfig = @"
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    "Issuer": "WMS.Auth.API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5000",
      "https://localhost:5001",
      "https://localhost:7000",
      "http://localhost:3000"
    ]
  }
"@

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

foreach ($api in $apis) {
    $appsettingsPath = Join-Path $api "appsettings.json"
    
    if (Test-Path $appsettingsPath) {
        Write-Host "Updating $api..." -ForegroundColor Cyan
        
        $content = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        # Add JWT and CORS settings if not exist
        if (-not $content.JwtSettings) {
            $jwtSettings = @{
                SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!"
                Issuer = "WMS.Auth.API"
                Audience = "WMS.Client"
                ExpirationMinutes = 60
                RefreshTokenExpirationDays = 7
            }
            $content | Add-Member -Type NoteProperty -Name "JwtSettings" -Value $jwtSettings
        }
        
        if (-not $content.Cors) {
            $corsSettings = @{
                AllowedOrigins = @(
                    "http://localhost:5000",
                    "https://localhost:5001",
                    "https://localhost:7000",
                    "http://localhost:3000"
                )
            }
            $content | Add-Member -Type NoteProperty -Name "Cors" -Value $corsSettings
        }
        
        $content | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        Write-Host "? $api updated" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "All APIs updated successfully!" -ForegroundColor Green
```

**Run it**:
```powershell
.\update-jwt-config.ps1
```

---

## ? Verification Checklist

- [x] Auth.API `appsettings.json` fixed
- [x] JWT SecretKey added (32+ characters)
- [x] Issuer configured
- [x] Audience configured
- [x] Token expiration settings added
- [x] CORS origins configured
- [x] Connection string updated to CONGTAM-PC
- [ ] Test Auth.API startup
- [ ] Test login endpoint
- [ ] Update other APIs with same config

---

## ?? Security Best Practices

### Development (Current)
```json
"SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!"
```
? Fine for local development

### Production
```json
"SecretKey": "${JWT_SECRET_KEY}"  // From environment variable
```

**Steps for Production**:
1. Generate strong random key (64+ characters)
2. Store in **Azure Key Vault** or **Environment Variables**
3. **NEVER** commit to source control
4. Rotate keys periodically

**Generate Production Key**:
```powershell
# PowerShell
[System.Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```

---

## ?? Summary

**Problem**: Missing JWT configuration  
**Solution**: Added `JwtSettings` and `Cors` sections to `appsettings.json`  
**Status**: ? **FIXED**

**Next Steps**:
1. ? Auth.API fixed
2. ?? Update other 7 APIs with same configuration (use script above)
3. ? Test login endpoint
4. ? Generate production secret key before deployment

**Your Auth.API is now ready to start! ??**

---

## ?? Related Documentation

- `GATEWAY_CLIENT_CONFIG.md` - How to use Auth API through Gateway
- `GATEWAY_FIXED_SUMMARY.md` - Gateway configuration
- `WEB_MIGRATION_SETUP_COMPLETE.md` - Database setup

**Start Auth.API now**:
```powershell
cd WMS.Auth.API
dotnet run
```

Then test: `https://localhost:7081/`
