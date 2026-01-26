# ? Global Exception Handler - Quick Implementation Guide

## ?? **Summary**

? **WMS.Tests** project added to solution  
? **Custom exception types** created in WMS.Domain  
? **GlobalExceptionHandler template** ready to use  

---

## ?? **2-Step Implementation Per API**

### **Step 1: Copy the GlobalExceptionHandler**

1. Create folder: `{APIProject}/Middleware/`
2. Copy template: `WMS.Domain/Templates/GlobalExceptionHandler.template.cs`
3. Rename to: `GlobalExceptionHandler.cs`
4. Update namespace to match your API (e.g., `WMS.Products.API.Middleware`)

### **Step 2: Register in Program.cs**

Add these 2 lines to each API's `Program.cs`:

```csharp
using YourAPINamespace.Middleware; // Add your middleware namespace

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>(); // ? ADD THIS
builder.Services.AddProblemDetails(); // ? ADD THIS

var app = builder.Build();

app.UseExceptionHandler(options => { }); // ? ADD THIS (before other middleware)
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## ?? **Custom Exceptions Available**

All in `WMS.Domain.Exceptions`:

```csharp
using WMS.Domain.Exceptions;

// 1. Business Rule Violation (HTTP 400)
throw new BusinessRuleViolationException("Cannot ship without payment");

// 2. Resource Not Found (HTTP 404)
throw new ResourceNotFoundException("Product", productId);

// 3. Validation Error (HTTP 400)
throw new ValidationException("SKU", "SKU already exists");

// 4. Concurrency Conflict (HTTP 409)
throw new ConcurrencyException("Resource was modified by another user");
```

---

## ? **API Projects Checklist**

- [ ] WMS.Auth.API
- [ ] WMS.Products.API
- [ ] WMS.Locations.API
- [ ] WMS.Inventory.API
- [ ] WMS.Inbound.API
- [ ] WMS.Outbound.API
- [ ] WMS.Payment.API
- [ ] WMS.Delivery.API

---

## ?? **Benefits**

? No more try-catch blocks in controllers  
? Automatic Serilog logging  
? Standardized error responses  
? Cleaner code  

**Effort:** 2 minutes per API × 8 APIs = 16 minutes total  
**Impact:** Removes ~200+ try-catch blocks

---

**See `WMS.Domain/GLOBAL_EXCEPTION_HANDLER.md` for detailed documentation**
