# ?? QUICK FIX GUIDE - Build Errors

## ? 2-Minute Manual Fix

All 8 microservice Program.cs files need a simple one-line fix on line 29.

---

## ?? Fix for 7 Microservices (Same Error)

**Files to Fix:**
1. `WMS.Inbound.API\Program.cs` - Line 29
2. `WMS.Outbound.API\Program.cs` - Line 29
3. `WMS.Inventory.API\Program.cs` - Line 29
4. `WMS.Locations.API\Program.cs` - Line 29
5. `WMS.Products.API\Program.cs` - Line 29
6. `WMS.Payment.API\Program.cs` - Line 29
7. `WMS.Delivery.API\Program.cs` - Line 29

### Current (Broken) Line 29:
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret
```

### Replace With (Fixed):
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
```

**How to Fix:**
1. Open the file in Visual Studio
2. Go to Line 29
3. Replace the entire line with the fixed version above
4. Save (Ctrl+S)
5. Repeat for all 7 files

---

## ?? Fix for WMS.Auth.API (Different Error)

**File:** `WMS.Auth.API\Program.cs`

### Fix 1: Line 29 (Same as above)
Replace:
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret
```

With:
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
```

### Fix 2: Line 92
Replace:
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

With:
```csharp
builder.Services.AddScoped<WMS.Application.Interfaces.IAuthService, AuthService>();
```

### Fix 3: Line 93
Replace:
```csharp
builder.Services.AddScoped<ITokenService>(sp => new TokenService(
```

With:
```csharp
builder.Services.AddScoped<WMS.Application.Interfaces.ITokenService>(sp => new TokenService(
```

---

## ? Even Faster: Search & Replace

### In Visual Studio:

1. **Press `Ctrl+Shift+H`** (Find and Replace in Files)

2. **First Replacement:**
   - Find: `throw new InvalidOperationException("JWT Secret`
   - Replace: `throw new InvalidOperationException("JWT SecretKey not configured");`
   - Look in: `Entire Solution`
   - Click `Replace All`

3. **Second Replacement (Auth API only):**
   - Find: `AddScoped<IAuthService, AuthService>`
   - Replace: `AddScoped<WMS.Application.Interfaces.IAuthService, AuthService>`
   - Click `Replace All`

4. **Third Replacement (Auth API only):**
   - Find: `AddScoped<ITokenService>(`
   - Replace: `AddScoped<WMS.Application.Interfaces.ITokenService>(`
   - Click `Replace All`

5. **Save All:** `Ctrl+Shift+S`

6. **Build:** `Ctrl+Shift+B`

---

## ? Verification

After fixing, run:
```powershell
dotnet build
```

**Expected Result:**
```
Build succeeded.
```

---

## ?? Summary of Errors

| Error Type | Count | Location |
|------------|-------|----------|
| Truncated exception message | 8 files | Line 29 (all Program.cs) |
| Interface mismatch | 2 lines | WMS.Auth.API lines 92-93 |

**Total:** 10 simple text replacements

**Time Required:** 2 minutes with Find & Replace

---

## ?? If Still Having Issues

1. **Close all files** in Visual Studio
2. **Close Visual Studio**
3. **Delete all `bin` and `obj` folders**:
   ```powershell
   Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
   ```
4. **Reopen solution** and try building again

---

**After fixing these, the build will succeed and we can continue with the CQRS implementation!** ??
