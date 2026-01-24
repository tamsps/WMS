# ?? Final Fix - Program.cs File Truncation Issue

## Problem Summary
The edit_file tool is truncating long string literals across multiple Program.cs files.

**Error Pattern:**
```csharp
// Line 29 gets truncated from:
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

// To this (missing end):
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret
```

## Affected Files (All 7 microservices)
1. WMS.Inbound.API\Program.cs - Line 29
2. WMS.Outbound.API\Program.cs - Line 29
3. WMS.Inventory.API\Program.cs - Line 29
4. WMS.Locations.API\Program.cs - Line 29
5. WMS.Products.API\Program.cs - Line 29
6. WMS.Payment.API\Program.cs - Line 29
7. WMS.Delivery.API\Program.cs - Line 29
8. WMS.Auth.API\Program.cs - Different errors (interface mismatch)

---

## ? SIMPLEST FIX - Manual Edit (2 Minutes)

### For each of the 7 microservices (Inbound, Outbound, Inventory, Locations, Products, Payment, Delivery):

**Open the Program.cs file and fix line 29:**

**FROM (broken):**
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret
```

**TO (fixed):**
```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
```

Just add: `Key not configured");` to the end of line 29.

### For WMS.Auth.API\Program.cs:

Fix line 92-93:
```csharp
// Change FROM:
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService>(sp => new TokenService(

// TO:
builder.Services.AddScoped<WMS.Application.Interfaces.IAuthService, AuthService>();
builder.Services.AddScoped<WMS.Application.Interfaces.ITokenService>(sp => new TokenService(
```

---

## ?? Quick Checklist

Open each file and fix line 29:

- [ ] WMS.Inbound.API\Program.cs - Line 29
- [ ] WMS.Outbound.API\Program.cs - Line 29
- [ ] WMS.Inventory.API\Program.cs - Line 29
- [ ] WMS.Locations.API\Program.cs - Line 29
- [ ] WMS.Products.API\Program.cs - Line 29  
- [ ] WMS.Payment.API\Program.cs - Line 29
- [ ] WMS.Delivery.API\Program.cs - Line 29
- [ ] WMS.Auth.API\Program.cs - Lines 92-93 (interface fix)

Then run: `dotnet build`

---

## Alternative: Complete Line 29 for All Files

If you prefer, here's the complete line 29 for copy-paste:

```csharp
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
```

---

## After Build Succeeds

Once the build is successful, we can continue with:

1. ? Verify all 8 microservices build successfully
2. ? Move services from Infrastructure to microservices
3. ? Implement CQRS pattern for WMS.Inbound.API (prototype)
4. ? Remove WMS.Application project
5. ? Final testing

---

## Status

**Current:** Build failing due to string truncation on line 29
**Fix Required:** Manual edit of 8 Program.cs files (2 minutes)
**Then:** Ready to continue with refactoring

**After you fix these, run `dotnet build` and let me know the result!**
