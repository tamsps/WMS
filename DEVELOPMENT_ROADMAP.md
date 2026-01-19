# Development Roadmap & Task Checklist

## Phase 1: Complete Core Services (Estimated: 2-3 days)

### Task 1.1: Implement InventoryService ⚠️ HIGH PRIORITY
**File:** `WMS.Infrastructure/Services/InventoryService.cs`  
**Status:** Template provided in IMPLEMENTATION_GUIDE.md  
**Effort:** 2-3 hours

**Checklist:**
- [ ] Copy implementation from IMPLEMENTATION_GUIDE.md
- [ ] Add method: UpdateInventoryAsync (for Inbound/Outbound to call)
- [ ] Test GetByIdAsync
- [ ] Test GetInventoryByProductAsync
- [ ] Test GetInventoryLevelsAsync with pagination
- [ ] Test GetTransactionsAsync with filters

### Task 1.2: Create InventoryController
**File:** `WMS.API/Controllers/InventoryController.cs`  
**Status:** To create  
**Effort:** 1 hour

**Endpoints to create:**
- [ ] GET /api/inventory/{id}
- [ ] GET /api/inventory
- [ ] GET /api/inventory/product/{productId}
- [ ] GET /api/inventory/levels
- [ ] GET /api/inventory/transactions

### Task 1.3: Implement InboundService
**File:** `WMS.Infrastructure/Services/InboundService.cs`  
**Status:** To implement  
**Effort:** 3-4 hours

**Methods to implement:**
- [ ] GetByIdAsync (with items)
- [ ] GetAllAsync (with pagination)
- [ ] CreateAsync (validate products, locations exist)
- [ ] ReceiveAsync (update inventory, use transactions)
- [ ] CancelAsync (with validation)

**Key Logic:**
```csharp
// In ReceiveAsync
1. Validate inbound exists and status is Pending
2. Begin transaction
3. Update InboundItems with received quantities
4. Call InventoryService.UpdateInventoryAsync for each item
5. Update Inbound status to Received
6. Commit transaction
7. Return success
```

### Task 1.4: Create InboundController
**File:** `WMS.API/Controllers/InboundController.cs`  
**Status:** To create  
**Effort:** 1 hour

**Endpoints:**
- [ ] POST /api/inbound
- [ ] GET /api/inbound/{id}
- [ ] GET /api/inbound
- [ ] POST /api/inbound/{id}/receive
- [ ] POST /api/inbound/{id}/cancel

### Task 1.5: Implement OutboundService
**File:** `WMS.Infrastructure/Services/OutboundService.cs`  
**Status:** To implement  
**Effort:** 4-5 hours

**Methods to implement:**
- [ ] GetByIdAsync
- [ ] GetAllAsync
- [ ] CreateAsync (validate inventory availability)
- [ ] PickAsync (reserve inventory)
- [ ] ShipAsync (deduct inventory, check payment if required)
- [ ] CancelAsync

**Key Logic:**
```csharp
// In ShipAsync
1. Validate outbound exists
2. Check payment status if required (call PaymentService)
3. Begin transaction
4. Deduct inventory for each item
5. Update outbound status to Shipped
6. Create delivery record if needed
7. Commit transaction
8. Return success
```

### Task 1.6: Create OutboundController
**File:** `WMS.API/Controllers/OutboundController.cs`  
**Status:** To create  
**Effort:** 1 hour

**Endpoints:**
- [ ] POST /api/outbound
- [ ] GET /api/outbound/{id}
- [ ] GET /api/outbound
- [ ] POST /api/outbound/{id}/pick
- [ ] POST /api/outbound/{id}/ship
- [ ] POST /api/outbound/{id}/cancel

## Phase 2: Payment & Delivery (Estimated: 2 days)

### Task 2.1: Implement PaymentService
**File:** `WMS.Infrastructure/Services/PaymentService.cs`  
**Effort:** 3-4 hours

**Methods:**
- [ ] GetByIdAsync
- [ ] GetAllAsync
- [ ] CreateAsync (link to outbound)
- [ ] InitiateAsync (prepare for payment gateway)
- [ ] ConfirmAsync (mark as paid)
- [ ] ProcessWebhookAsync (handle gateway callbacks)
- [ ] CanShipAsync (check if payment allows shipment)

**Payment Logic:**
```csharp
// CanShipAsync logic
- Prepaid: Must be Confirmed
- COD: Always true (paid on delivery)
- Postpaid: Always true (invoice later)
```

### Task 2.2: Create PaymentController
**File:** `WMS.API/Controllers/PaymentController.cs`  
**Effort:** 1 hour

**Endpoints:**
- [ ] POST /api/payment
- [ ] GET /api/payment/{id}
- [ ] GET /api/payment
- [ ] POST /api/payment/{id}/initiate
- [ ] POST /api/payment/{id}/confirm
- [ ] POST /api/payment/webhook (AllowAnonymous for gateway)

### Task 2.3: Implement DeliveryService
**File:** `WMS.Infrastructure/Services/DeliveryService.cs`  
**Effort:** 3 hours

**Methods:**
- [ ] GetByIdAsync (with events)
- [ ] GetAllAsync
- [ ] CreateAsync (link to outbound)
- [ ] UpdateStatusAsync (add event)
- [ ] CompleteAsync (mark delivered)
- [ ] FailAsync (handle failure, prepare for return)
- [ ] GetByTrackingNumberAsync

### Task 2.4: Create DeliveryController
**File:** `WMS.API/Controllers/DeliveryController.cs`  
**Effort:** 1 hour

**Endpoints:**
- [ ] POST /api/delivery
- [ ] GET /api/delivery/{id}
- [ ] GET /api/delivery
- [ ] GET /api/delivery/tracking/{trackingNumber}
- [ ] PUT /api/delivery/{id}/status
- [ ] POST /api/delivery/{id}/complete
- [ ] POST /api/delivery/{id}/fail

## Phase 3: Authentication (Estimated: 1 day)

### Task 3.1: Install Password Hashing Package
```powershell
cd WMS.Infrastructure
dotnet add package BCrypt.Net-Next
```

### Task 3.2: Implement AuthService
**File:** `WMS.Infrastructure/Services/AuthService.cs`  
**Effort:** 2-3 hours

**Methods:**
- [ ] LoginAsync (validate credentials, generate tokens)
- [ ] RegisterAsync (hash password, create user)
- [ ] RefreshTokenAsync (validate and refresh)
- [ ] LogoutAsync (clear refresh token)

**Example:**
```csharp
public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
{
    var user = await _context.Users
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Username == dto.Username);
    
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Result<LoginResponseDto>.Failure("Invalid credentials");
    
    var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
    var token = _tokenService.GenerateAccessToken(user, roles);
    var refreshToken = _tokenService.GenerateRefreshToken();
    
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
    user.LastLoginDate = DateTime.UtcNow;
    
    await _unitOfWork.SaveChangesAsync();
    
    return Result<LoginResponseDto>.Success(new LoginResponseDto
    {
        Token = token,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
        User = MapToUserDto(user, roles)
    });
}
```

### Task 3.3: Create AuthController
**File:** `WMS.API/Controllers/AuthController.cs`  
**Effort:** 1 hour

**Endpoints:**
- [ ] POST /api/auth/login (AllowAnonymous)
- [ ] POST /api/auth/register (AllowAnonymous)
- [ ] POST /api/auth/refresh (AllowAnonymous)
- [ ] POST /api/auth/logout (Authorized)

### Task 3.4: Update DbContext Seed Data
**File:** `WMS.Infrastructure/Data/WMSDbContext.cs`  
**Effort:** 30 minutes

- [ ] Replace plain password with BCrypt hash
```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
```

### Task 3.5: Update Program.cs
**File:** `WMS.API/Program.cs`  
**Effort:** 10 minutes

- [ ] Add AuthService registration
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

## Phase 4: Validation & Error Handling (Estimated: 1 day)

### Task 4.1: Create FluentValidation Validators
**Location:** `WMS.Application/Validators/`

**Files to create:**
- [ ] CreateProductDtoValidator.cs
- [ ] UpdateProductDtoValidator.cs
- [ ] CreateLocationDtoValidator.cs
- [ ] CreateInboundDtoValidator.cs
- [ ] CreateOutboundDtoValidator.cs
- [ ] LoginDtoValidator.cs
- [ ] RegisterDtoValidator.cs

**Example:**
```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);
        
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");
    }
}
```

### Task 4.2: Register Validators in Program.cs
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
```

### Task 4.3: Create Global Exception Handler
**File:** `WMS.API/Middleware/GlobalExceptionMiddleware.cs`

### Task 4.4: Add Validation Middleware to Controllers
Use `[ApiController]` attribute (already done) or create validation filter.

## Phase 5: Testing (Estimated: 2 days)

### Task 5.1: Integration Testing
**Create test project:**
```powershell
dotnet new xunit -n WMS.Tests
dotnet sln add WMS.Tests/WMS.Tests.csproj
```

**Test scenarios:**
- [ ] Complete workflow: Create Product → Create Location → Inbound → Outbound
- [ ] Negative inventory prevention
- [ ] Payment gating for prepaid orders
- [ ] Concurrent outbound requests
- [ ] Authentication flow

### Task 5.2: Manual API Testing Checklist

**Authentication:**
- [ ] Register new user
- [ ] Login with correct credentials
- [ ] Login with incorrect credentials
- [ ] Access protected endpoint without token
- [ ] Access protected endpoint with valid token
- [ ] Refresh token

**Product Flow:**
- [ ] Create product with valid data
- [ ] Create product with duplicate SKU (should fail)
- [ ] Get product by ID
- [ ] Get product by SKU
- [ ] Search products
- [ ] Update product
- [ ] Deactivate product
- [ ] Try to use deactivated product in transaction (should fail)

**Location Flow:**
- [ ] Create location
- [ ] Create location with duplicate code (should fail)
- [ ] Get locations by zone
- [ ] Update location capacity
- [ ] Try to deactivate location with inventory (should fail)

**Inbound Flow:**
- [ ] Create inbound order
- [ ] Receive goods (full quantity)
- [ ] Receive goods (partial quantity)
- [ ] Check inventory increased
- [ ] Check transaction recorded
- [ ] Cancel inbound

**Outbound Flow:**
- [ ] Create outbound order (validate inventory available)
- [ ] Create outbound with insufficient inventory (should fail)
- [ ] Pick items
- [ ] Ship items (check inventory decreased)
- [ ] Verify cannot ship below zero
- [ ] Cancel outbound

**Payment Flow:**
- [ ] Create payment for outbound
- [ ] Try to ship prepaid order without payment (should fail)
- [ ] Confirm payment
- [ ] Ship prepaid order (should succeed)
- [ ] Ship COD order without payment (should succeed)

**Inventory Flow:**
- [ ] View inventory by product
- [ ] View inventory by location
- [ ] View transaction history
- [ ] Verify balance calculations

## Phase 6: Web UI (Estimated: 1 week)

### Task 6.1: Create MVC Controllers
**Location:** `WMS.Web/Controllers/`

- [ ] HomeController (dashboard)
- [ ] ProductsController (CRUD views)
- [ ] LocationsController (CRUD views)
- [ ] InboundController (create, receive)
- [ ] OutboundController (create, pick, ship)
- [ ] InventoryController (view, search)
- [ ] AccountController (login, logout)

### Task 6.2: Create Views
**Location:** `WMS.Web/Views/`

- [ ] Shared/_Layout.cshtml (navigation)
- [ ] Home/Index.cshtml (dashboard)
- [ ] Products/Index.cshtml (list)
- [ ] Products/Create.cshtml (form)
- [ ] Products/Edit.cshtml (form)
- [ ] And more...

### Task 6.3: Add HTTP Client for API Calls
```csharp
builder.Services.AddHttpClient("WMSAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7xxx");
});
```

## Critical Path Items

### MUST DO (Before deployment)
1. ✅ Complete all service implementations
2. ✅ Create all controllers
3. ✅ Implement authentication
4. ✅ Add validation
5. ✅ Test critical workflows
6. ✅ Handle errors properly
7. ✅ Secure API endpoints
8. ✅ Test with realistic data

### SHOULD DO (Important but not blocking)
1. Add logging (Serilog)
2. Add health checks
3. Add API versioning
4. Optimize database queries
5. Add caching where appropriate
6. Performance testing
7. Security audit

### NICE TO HAVE (Future enhancements)
1. Advanced reporting
2. Email notifications
3. Export to Excel
4. Barcode scanning
5. Mobile app
6. Real-time updates (SignalR)
7. Integration with external systems

## Estimated Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Core Services | 2-3 days | None |
| Phase 2: Payment & Delivery | 2 days | Phase 1 |
| Phase 3: Authentication | 1 day | None (can parallel) |
| Phase 4: Validation | 1 day | Phase 1-3 |
| Phase 5: Testing | 2 days | Phase 1-4 |
| Phase 6: Web UI | 1 week | Phase 1-5 |
| **Total** | **2-3 weeks** | Sequential + some parallel |

## Success Metrics

### Code Quality
- [ ] All services follow established patterns
- [ ] Consistent error handling
- [ ] Proper use of async/await
- [ ] No code duplication
- [ ] Comments on complex logic

### Functionality
- [ ] All CRUD operations work
- [ ] Transactions are atomic
- [ ] Inventory cannot go negative
- [ ] Payment gates shipment correctly
- [ ] Audit trail is complete

### Security
- [ ] JWT authentication works
- [ ] Authorization is enforced
- [ ] Passwords are hashed
- [ ] SQL injection prevented (EF Core)
- [ ] CORS configured correctly

### Performance
- [ ] API responds in < 500ms
- [ ] Pagination works efficiently
- [ ] Database queries are optimized
- [ ] No N+1 query problems

## Resources Needed

### Development Tools
- Visual Studio 2022 or VS Code
- SQL Server Management Studio
- Postman or similar API testing tool
- Git for version control

### Documentation
- ✅ README.md (provided)
- ✅ QUICK_START.md (provided)
- ✅ IMPLEMENTATION_GUIDE.md (provided)
- ✅ PROJECT_SUMMARY.md (provided)
- ✅ DEVELOPMENT_ROADMAP.md (this file)

### Team
- 1-2 Backend Developers
- 1 Frontend Developer (for Web UI)
- 1 QA Tester
- 1 DevOps (for deployment)

## Getting Help

### When Stuck
1. Check existing implementations (ProductService, LocationService)
2. Review IMPLEMENTATION_GUIDE.md
3. Check README.md for API patterns
4. Review Entity Framework documentation
5. Check ASP.NET Core documentation

### Common Issues & Solutions
1. **DbContext error**: Check connection string
2. **Migration fails**: Delete migrations folder and recreate
3. **JWT not working**: Check secret key length (min 32 chars)
4. **CORS error**: Check allowed origins in appsettings.json
5. **Build error**: Run `dotnet restore` and rebuild

---

## Next Immediate Step

**START HERE:**
```powershell
# 1. Create database
cd WMS.API
dotnet ef migrations add InitialCreate
dotnet ef database update

# 2. Run API
dotnet run

# 3. Test in Swagger (https://localhost:7xxx/)
# Try creating a product and location

# 4. Begin Phase 1, Task 1.1 (InventoryService)
```

Good luck! Follow this roadmap and you'll have a complete system in 2-3 weeks! 🚀
