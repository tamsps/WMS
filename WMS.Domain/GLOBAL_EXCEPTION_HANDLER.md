# Global Exception Handler - Implementation Guide

## ?? **Overview**

The WMS system now has a **global exception handler** that automatically catches and handles ALL unhandled exceptions across all API projects. This eliminates the need for try-catch blocks in controllers and command handlers.

## ? **Benefits**

1. ? **No More Try-Catch Blocks**: Exception handling is centralized
2. ? **Consistent Error Responses**: Standardized RFC 7807 Problem Details format
3. ? **Automatic Logging**: All exceptions logged with Serilog automatically
4. ? **Custom Exceptions**: Business-specific exception types
5. ? **Clean Code**: Handlers focus on business logic only

---

## ?? **Setup Instructions**

### **Step 1: Add to Program.cs**

In **each API project's Program.cs**, add the following:

```csharp
using WMS.Domain.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// ? ADD THIS: Register global exception handler
builder.Services.AddGlobalExceptionHandler();

var app = builder.Build();

// ? ADD THIS: Use global exception handler (add BEFORE other middleware)
app.UseGlobalExceptionHandler();

// Other middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**IMPORTANT**: `app.UseGlobalExceptionHandler()` must be called **BEFORE** other middleware to catch all exceptions.

---

## ?? **Implementation Status**

### **API Projects to Update**

| Project | Program.cs Updated | Status |
|---------|-------------------|--------|
| WMS.Auth.API | ? Pending | Update Required |
| WMS.Products.API | ? Pending | Update Required |
| WMS.Locations.API | ? Pending | Update Required |
| WMS.Inventory.API | ? Pending | Update Required |
| WMS.Inbound.API | ? Pending | Update Required |
| WMS.Outbound.API | ? Pending | Update Required |
| WMS.Payment.API | ? Pending | Update Required |
| WMS.Delivery.API | ? Pending | Update Required |

---

## ?? **Custom Exceptions**

The global exception handler supports these custom exception types:

### **1. BusinessRuleViolationException** (HTTP 400)
For business logic violations:
```csharp
// BEFORE (with try-catch):
try
{
    if (payment.PaymentType == PaymentType.Prepaid && payment.Status != PaymentStatus.Confirmed)
    {
        return Result.Failure("Cannot ship prepaid order without payment confirmation");
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    return Result.Failure(ex.Message);
}

// AFTER (no try-catch needed):
if (payment.PaymentType == PaymentType.Prepaid && payment.Status != PaymentStatus.Confirmed)
{
    throw new BusinessRuleViolationException("Cannot ship prepaid order without payment confirmation");
}
// Exception handler catches it, logs it, returns 400 Bad Request automatically
```

### **2. ResourceNotFoundException** (HTTP 404)
For resources that don't exist:
```csharp
// BEFORE:
var product = await _context.Products.FindAsync(productId);
if (product == null)
{
    return Result.Failure("Product not found");
}

// AFTER:
var product = await _context.Products.FindAsync(productId);
if (product == null)
{
    throw new ResourceNotFoundException("Product", productId);
}
// Returns: 404 Not Found - "Product with ID 'xxx' was not found."
```

### **3. ValidationException** (HTTP 400)
For validation errors:
```csharp
// Single property validation:
if (string.IsNullOrEmpty(dto.SKU))
{
    throw new ValidationException("SKU", "SKU is required");
}

// Multiple property validation:
var errors = new Dictionary<string, string[]>
{
    ["SKU"] = new[] { "SKU is required", "SKU must be unique" },
    ["Name"] = new[] { "Name is required" }
};
throw new ValidationException(errors);
```

### **4. ConcurrencyException** (HTTP 409)
For optimistic concurrency conflicts:
```csharp
// The handler automatically converts DbUpdateConcurrencyException to HTTP 409
// But you can also throw explicitly:
throw new ConcurrencyException("Inventory was modified during shipping. Please refresh and try again.");
```

---

## ?? **Error Response Format**

All exceptions return standardized RFC 7807 Problem Details:

### **Example Response:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Business Rule Violation",
  "status": 400,
  "detail": "Cannot ship prepaid order without payment confirmation",
  "instance": "/api/outbound/ship",
  "traceId": "00-1234567890abcdef-1234567890abcdef-00",
  "timestamp": "2025-01-25 14:30:00 UTC",
  "exceptionType": "BusinessRuleViolationException"
}
```

### **Validation Error Response:**
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/products",
  "traceId": "00-abcdef1234567890-abcdef1234567890-00",
  "timestamp": "2025-01-25 14:30:00 UTC",
  "exceptionType": "ValidationException",
  "errors": {
    "SKU": ["SKU is required", "SKU must be unique"],
    "Name": ["Name is required"]
  }
}
```

---

## ?? **Exception Mapping**

The handler automatically maps exceptions to HTTP status codes:

| Exception Type | HTTP Status | Description |
|----------------|-------------|-------------|
| **BusinessRuleViolationException** | 400 Bad Request | Business logic violations |
| **ValidationException** | 400 Bad Request | Validation errors |
| **ResourceNotFoundException** | 404 Not Found | Resource doesn't exist |
| **ConcurrencyException** | 409 Conflict | Optimistic locking conflict |
| **DbUpdateConcurrencyException** | 409 Conflict | EF Core concurrency |
| **ArgumentNullException** | 400 Bad Request | Null arguments |
| **ArgumentException** | 400 Bad Request | Invalid arguments |
| **InvalidOperationException** | 400 Bad Request | Invalid operations |
| **KeyNotFoundException** | 404 Not Found | Key not found in collection |
| **UnauthorizedAccessException** | 401 Unauthorized | Authorization failed |
| **TimeoutException** | 408 Request Timeout | Operation timed out |
| **All Others** | 500 Internal Server Error | Unexpected errors |

---

## ?? **Code Cleanup Examples**

### **Before (with try-catch):**
```csharp
public async Task<Result<OutboundDto>> Handle(ShipOutboundCommand request, ...)
{
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        var outbound = await _context.Outbounds.FindAsync(request.Dto.OutboundId);
        if (outbound == null)
        {
            return Result.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Picked)
        {
            return Result.Failure($"Cannot ship outbound in {outbound.Status} status");
        }

        // Business logic...
        await _unitOfWork.CommitTransactionAsync();

        return Result.Success(OutboundMapper.MapToDto(outbound));
    }
    catch (DbUpdateConcurrencyException)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return Result.Failure("Inventory was modified by another user");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error shipping outbound");
        return Result.Failure($"Failed to ship outbound: {ex.Message}");
    }
}
```

### **After (no try-catch):**
```csharp
public async Task<Result<OutboundDto>> Handle(ShipOutboundCommand request, ...)
{
    await _unitOfWork.BeginTransactionAsync();

    var outbound = await _context.Outbounds.FindAsync(request.Dto.OutboundId);
    if (outbound == null)
    {
        throw new ResourceNotFoundException("Outbound", request.Dto.OutboundId);
    }

    if (outbound.Status != OutboundStatus.Picked)
    {
        throw new BusinessRuleViolationException($"Cannot ship outbound in {outbound.Status} status");
    }

    // Business logic...
    await _unitOfWork.CommitTransactionAsync();

    return Result.Success(OutboundMapper.MapToDto(outbound));
    
    // ? No try-catch needed!
    // ? DbUpdateConcurrencyException automatically caught, logged, returns 409
    // ? Any exception automatically rolled back, logged, returns appropriate status
}
```

---

## ?? **Logging**

All exceptions are automatically logged with Serilog (or any ILogger implementation) including:

- Exception type
- Exception message
- Stack trace
- Request path
- HTTP method
- Trace ID (for correlation)
- Timestamp

**Log Example:**
```
2025-01-25 14:30:00 [Error] Unhandled exception: BusinessRuleViolationException | Message: Cannot ship prepaid order without payment confirmation | Path: /api/outbound/ship | Method: POST | TraceId: 00-1234567890abcdef-1234567890abcdef-00
```

---

## ?? **Important Notes**

### **1. Transaction Rollback**
The exception handler **does NOT automatically rollback transactions**. You should handle transaction rollback in your UnitOfWork or use a transaction scope that auto-rolls back on exceptions.

**Recommended Pattern:**
```csharp
public async Task<Result> Handle(...)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync();
    
    // Business logic - any exception will rollback automatically
    
    await transaction.CommitAsync();
    return Result.Success();
}
```

### **2. Controller Return Types**
Controllers can now be simplified since exceptions are handled globally:

```csharp
// BEFORE:
[HttpPost("ship")]
public async Task<IActionResult> Ship([FromBody] ShipOutboundDto dto)
{
    try
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error");
        return StatusCode(500, "Internal server error");
    }
}

// AFTER (exceptions handled automatically):
[HttpPost("ship")]
public async Task<IActionResult> Ship([FromBody] ShipOutboundDto dto)
{
    var result = await _mediator.Send(command);
    if (!result.IsSuccess)
    {
        return BadRequest(result);
    }
    return Ok(result);
    // ? Any unhandled exception is caught by global handler
}
```

### **3. Result Pattern Still Valid**
The `Result<T>` pattern is still valuable for **expected failures** (business validation). Use exceptions for **unexpected errors**.

**Guideline:**
- ? Use `Result.Failure()` for: Expected validation failures that are part of business flow
- ? Use `throw Exception` for: Unexpected errors, missing resources, constraint violations

---

## ?? **Testing**

The exception handler is designed for production. In tests, exceptions will be thrown normally and can be caught by your test framework.

```csharp
[Fact]
public async Task Handle_WithInvalidStatus_ShouldThrowBusinessRuleViolation()
{
    // Arrange
    var command = new ShipOutboundCommand { /* ... */ };
    
    // Act & Assert
    await Assert.ThrowsAsync<BusinessRuleViolationException>(
        async () => await _handler.Handle(command, CancellationToken.None));
}
```

---

## ? **Checklist for Implementation**

- [ ] Add `builder.Services.AddGlobalExceptionHandler()` to each API Program.cs
- [ ] Add `app.UseGlobalExceptionHandler()` to each API Program.cs (before other middleware)
- [ ] Remove unnecessary try-catch blocks from controllers
- [ ] Replace `Result.Failure()` with custom exceptions where appropriate
- [ ] Test exception handling in each API
- [ ] Verify Serilog logging is working
- [ ] Update integration tests to expect Problem Details responses

---

## ?? **Additional Resources**

- **RFC 7807 Problem Details**: https://tools.ietf.org/html/rfc7807
- **IExceptionHandler Interface**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
- **ASP.NET Core Exception Handling**: https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors

---

**Created:** 2025-01-25  
**Status:** ? Ready for Implementation  
**Impact:** Removes need for try-catch in ~100+ methods across all APIs
