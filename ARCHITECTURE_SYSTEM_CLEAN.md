# WMS (Warehouse Management System) - Clean Architecture

**Version**: 2.0  
**Date**: January 28, 2026  
**Status**: Production Ready  
**Pattern**: Clean Architecture + Microservices + API Gateway

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Architectural Layers](#architectural-layers)
3. [Microservices Design](#microservices-design)
4. [Service Responsibilities](#service-responsibilities)
5. [Communication Patterns](#communication-patterns)
6. [Security Architecture](#security-architecture)
7. [Scalability & Performance](#scalability--performance)

---

## Architecture Overview

The WMS system employs **Clean Architecture** principles to ensure maintainability, testability, and scalability across 8 independent microservices unified by an API Gateway.

### System Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                    CLIENT LAYER                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │  Web UI (MVC)    │   Mobile App   │   Admin Dashboard     │  │
│  │  Port: 5001      │   Port: 5008   │   Port: 5011          │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────┬───────────────────────────────────────┘
                           │ HTTP/HTTPS
                           ▼
              ┌────────────────────────────┐
              │   API GATEWAY (YARP)       │
              │   Port: 5000               │
              │  • Request Routing         │
              │  • Auth/Authorization      │
              │  • Rate Limiting           │
              │  • Load Balancing          │
              └────────────────┬───────────┘
                               │
    ┌──────────┬──────────┬──────────┬──────────┬──────────┬──────────┬──────────┬──────────┐
    │          │          │          │          │          │          │          │          │
    ▼          ▼          ▼          ▼          ▼          ▼          ▼          ▼          ▼
┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌─────────┐┌──────────┐┌──────────┐
│ Auth   ││Product ││Location││Inbound ││Outbound││Payment ││Delivery ││Inventory││ Reserved │
│ API    ││API     ││API     ││API     ││API     ││API     ││API      ││API      ││(Future)  │
│5002    ││5003    ││5004    ││5005    ││5006    ││5007    ││5009     ││5010     ││ Service  │
└────────┘└────────┘└────────┘└────────┘└────────┘└────────┘└─────────┘└──────────┘└──────────┘
    │          │          │          │          │          │          │          │
    └──────────┴──────────┴──────────┴──────────┴──────────┴──────────┴──────────┴──────────┘
                                     │
                    ┌────────────────────────────┐
                    │   SQL Server Database      │
                    │   (WMSDB / LocalDB)        │
                    │   Unified Schema           │
                    └────────────────────────────┘
```

---

## Architectural Layers

Each microservice follows Clean Architecture with 4 distinct layers:

### Layer 1: Domain Layer (Enterprise Business Rules)
**Location**: `WMS.Domain/`  
**Responsibility**: Core business entities and rules  

**Contains**:
- `Entities/` - Core business objects
  - `User`, `Role`, `Permission`
  - `Product`, `SKU`, `Barcode`
  - `Location`, `Zone`, `Aisle`
  - `InventoryRecord`, `Transaction`
  - `InboundOrder`, `OutboundOrder`
  - `PaymentTransaction`, `DeliveryRoute`
- `ValueObjects/` - Immutable objects
  - `Money`, `Quantity`, `Dimension`
  - `Address`, `ContactInfo`
- `Enums/` - Business constants
  - `OrderStatus`, `LocationType`, `UserRole`
  - `TransactionType`, `PaymentStatus`
- `Interfaces/` - Core contracts (repository interfaces)

**Key Principle**: No external dependencies; pure C# code

---

### Layer 2: Application Layer (Use Cases)
**Location**: `WMS.[Service].Application/`  
**Responsibility**: Business logic and orchestration  

**Contains**:
- `Services/` - Application services
  - `IAuthService`, `ITokenService`
  - `IProductService`, `IInventoryService`
  - `IOrderService`, `IPaymentService`
- `DTOs/` - Data Transfer Objects
  - Request/Response models for API
  - Validation attributes
- `Mappers/` - Object transformations
  - Entity → DTO conversions
  - DTO → Entity conversions
- `Validators/` - Business rule validation
  - Fluent Validation rules
  - Custom validators

**Key Principle**: No knowledge of presentation or data access

---

### Layer 3: Infrastructure Layer (Implementation Details)
**Location**: `WMS.[Service].API/` (partially) + EntityFramework implementations  
**Responsibility**: Data access, external services, persistence  

**Contains**:
- `Services/` - Implementation of application interfaces
  - Database operations
  - External API calls
  - Caching logic
- `Persistence/` - Data access
  - `DbContext` configuration
  - Repository implementations
  - EF Core migrations
- `Authentication/` - JWT & security
  - Token generation
  - Claims mapping
  - Policy handlers

**Key Principle**: All external dependencies here; replaceable implementations

---

### Layer 4: Presentation Layer (Controllers & Views)
**Location**: `WMS.[Service].API/Controllers` + `WMS.Web/`  
**Responsibility**: HTTP endpoints and user interface  

**API Services** (`WMS.[Service].API/`):
- `Controllers/` - REST endpoints
  - `AuthController`
  - `ProductController`
  - `LocationController`
  - etc.
- `Program.cs` - Dependency Injection setup
- `appsettings.json` - Configuration

**Web Service** (`WMS.Web/`):
- `Controllers/` - MVC actions
- `Views/` - Razor templates
- `Models/` - ViewModel classes

**Key Principle**: Thin controllers; delegate to services

---

## Microservices Design

### 1. WMS.Auth.API (Authentication & Authorization)
**Port**: 5002  
**Database**: Shared WMSDB  
**Primary Responsibility**: User identity and access control  

```
WMS.Auth.API/
├── Controllers/
│   └── AuthController (login, register, refresh, logout, profile)
├── Application/
│   └── IAuthService, ITokenService
├── Services/
│   ├── AuthService (credentials validation)
│   ├── TokenService (JWT generation)
│   └── UserService (user management)
└── [DTOs, Models, Configuration]
```

**Key Endpoints**:
- `POST /api/auth/login` - User authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh-token` - Token refresh
- `GET /api/auth/me` - Current user profile
- `POST /api/auth/logout` - Logout

---

### 2. WMS.Products.API (Product Catalog)
**Port**: 5003  
**Database**: Shared WMSDB  
**Primary Responsibility**: Product management and SKU operations  

```
WMS.Products.API/
├── Controllers/
│   ├── ProductController
│   └── CategoryController
├── Application/
│   ├── IProductService
│   └── ICategoryService
├── Services/
│   ├── ProductService
│   └── CategoryService
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `GET /api/products` - List products (paginated)
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `PATCH /api/products/{id}/activate` - Activate product

---

### 3. WMS.Locations.API (Warehouse Locations)
**Port**: 5004  
**Database**: Shared WMSDB  
**Primary Responsibility**: Physical location hierarchy  

```
WMS.Locations.API/
├── Controllers/
│   └── LocationController
├── Application/
│   └── ILocationService
├── Services/
│   └── LocationService
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `GET /api/locations` - List all locations
- `GET /api/locations/{id}` - Get location details
- `POST /api/locations` - Create location
- `PUT /api/locations/{id}` - Update location
- `PATCH /api/locations/{id}/activate` - Activate location

---

### 4. WMS.Inventory.API (Stock Management)
**Port**: 5010  
**Database**: Shared WMSDB  
**Primary Responsibility**: Real-time inventory tracking  

```
WMS.Inventory.API/
├── Controllers/
│   ├── InventoryController
│   └── TransactionController
├── Application/
│   ├── IInventoryService
│   └── ITransactionService
├── Services/
│   ├── InventoryService (stock updates)
│   └── TransactionService (audit trail)
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `GET /api/inventory` - All inventory records
- `GET /api/inventory/{id}` - Specific record
- `GET /api/inventory/product/{productId}` - Product stock
- `GET /api/inventory/levels` - Stock summary
- `POST /api/inventory/adjust` - Adjust stock

---

### 5. WMS.Inbound.API (Receiving)
**Port**: 5005  
**Database**: Shared WMSDB  
**Primary Responsibility**: Goods receiving and putaway  

```
WMS.Inbound.API/
├── Controllers/
│   └── InboundController
├── Application/
│   └── IInboundService
├── Services/
│   └── InboundService (receives → putaway → stock)
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `POST /api/inbound/create-order` - Create receive order
- `GET /api/inbound/{orderId}` - Get order details
- `POST /api/inbound/{orderId}/receive-item` - Receive item
- `POST /api/inbound/{orderId}/putaway` - Put item in location

---

### 6. WMS.Outbound.API (Shipping)
**Port**: 5006  
**Database**: Shared WMSDB  
**Primary Responsibility**: Order fulfillment and picking  

```
WMS.Outbound.API/
├── Controllers/
│   └── OutboundController
├── Application/
│   └── IOutboundService
├── Services/
│   └── OutboundService (pick → pack → ship)
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `POST /api/outbound/create-order` - Create ship order
- `GET /api/outbound/{orderId}` - Get order details
- `POST /api/outbound/{orderId}/pick` - Pick items
- `POST /api/outbound/{orderId}/pack` - Pack shipment
- `POST /api/outbound/{orderId}/ship` - Ship order

---

### 7. WMS.Payment.API (Payment Processing)
**Port**: 5007  
**Database**: Shared WMSDB  
**Primary Responsibility**: Payment transactions and reconciliation  

```
WMS.Payment.API/
├── Controllers/
│   ├── PaymentController
│   └── ReconciliationController
├── Application/
│   ├── IPaymentService
│   └── IReconciliationService
├── Services/
│   ├── PaymentService
│   └── ReconciliationService
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `POST /api/payment/process` - Process payment
- `GET /api/payment/{transactionId}` - Get transaction
- `POST /api/payment/reconcile` - Reconcile payments
- `GET /api/payment/history` - Payment history

---

### 8. WMS.Delivery.API (Delivery Management)
**Port**: 5009  
**Database**: Shared WMSDB  
**Primary Responsibility**: Delivery routing and tracking  

```
WMS.Delivery.API/
├── Controllers/
│   ├── DeliveryController
│   └── RouteController
├── Application/
│   ├── IDeliveryService
│   └── IRouteService
├── Services/
│   ├── DeliveryService
│   └── RouteService
└── [DTOs, Models, Validation]
```

**Key Endpoints**:
- `POST /api/delivery/create-route` - Create delivery route
- `GET /api/delivery/routes` - List routes
- `POST /api/delivery/{routeId}/track` - Track delivery
- `POST /api/delivery/{routeId}/complete` - Complete delivery

---

## Communication Patterns

### REST API Communication
- **Protocol**: HTTP/HTTPS
- **Format**: JSON
- **Authentication**: JWT Bearer Token (via API Gateway)
- **Gateway**: YARP reverse proxy at `Port: 5000`

### Request Flow
```
1. Client → API Gateway (Port 5000)
2. Gateway validates JWT token
3. Gateway routes to appropriate service
4. Service processes and returns JSON response
5. Response → Client
```

### Service-to-Service (Future Enhancement)
```
Option 1: Synchronous HTTP calls (current)
- Direct REST calls between services
- Simple but tight coupling

Option 2: Asynchronous messaging (recommended)
- RabbitMQ / Azure Service Bus
- Event-driven architecture
- Loose coupling
```

---

## Security Architecture

### Authentication Flow
```
1. User credentials → /api/auth/login
2. Auth API validates against User table
3. JWT token generated with claims:
   - sub (user ID)
   - username
   - roles
   - permissions
   - iat (issued at)
   - exp (expiration - 1 hour)
4. Client stores JWT in localStorage
5. Subsequent requests include: Authorization: Bearer {token}
```

### Authorization Levels
```
Role-Based Access Control (RBAC):
├── Admin (all permissions)
├── Manager (department-level)
├── Supervisor (team-level)
├── Operator (task-level)
└── Viewer (read-only)
```

### Security Headers
```
- Content-Security-Policy
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- X-XSS-Protection
- Strict-Transport-Security (HTTPS only)
```

---

## Scalability & Performance

### Horizontal Scaling
Each microservice can be deployed independently:
```
WMS.Auth.API
├── Instance 1 (Port 5002)
├── Instance 2 (Port 5012)
└── Instance 3 (Port 5022)
[Load Balancer]

(Same for other services)
```

### Caching Strategy
```
Layer 1: In-Memory Cache (IMemoryCache)
  - User roles/permissions
  - Product catalog (30-minute TTL)
  
Layer 2: Distributed Cache (Redis - future)
  - Session tokens
  - Frequently accessed data
  
Layer 3: Database Query Cache
  - EF Core compiled queries
  - Indexed frequently searched fields
```

### Database Optimization
```
✓ Indexed columns: Id, UserId, ProductId, LocationId
✓ Partitioned large tables by date range
✓ Async/await throughout application
✓ Pagination (default 20 items per page)
✓ Connection pooling (configured in appsettings)
```

---

## Dependency Management

### Core NuGet Packages
```
Framework:
  - Microsoft.AspNetCore.App (net9.0)
  - Microsoft.EntityFrameworkCore (9.0)
  - Microsoft.EntityFrameworkCore.SqlServer

Authentication:
  - System.IdentityModel.Tokens.Jwt
  - Microsoft.AspNetCore.Authentication.JwtBearer

Validation:
  - FluentValidation

API Gateway:
  - Yarp.ReverseProxy

Utilities:
  - AutoMapper
  - Serilog (logging)
  - Swashbuckle (OpenAPI/Swagger)
```

---

## Data Flow Example: Create Inbound Order

```
1. User submits form in WMS.Web
   POST /api/inbound/orders

2. Request → API Gateway (Port 5000)
   - Validates JWT token
   - Routes to WMS.Inbound.API:5005

3. InboundController receives request
   - Calls InboundService.CreateOrder()

4. InboundService (Application Layer)
   - Validates business rules
   - Calls ValidateOrder() [Domain Layer]
   - Calls _inboundRepository.AddAsync() [Infra Layer]

5. EF Core saves to WMSDB
   - Inserts InboundOrder record
   - Generates InboundOrderId

6. Service returns OrderDto

7. Controller returns ApiResponse
   { success: true, data: { orderId: "..." } }

8. API Gateway → Web UI
   - Web updates UI
   - User sees confirmation
```

---

## Development Guidelines

### Adding a New Endpoint

1. **Define DTO** in `Service.API/DTOs/`
```csharp
public class CreateProductRequest
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
}
```

2. **Create Interface** in `Service.Application/`
```csharp
public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request);
}
```

3. **Implement Service** in `Service.API/Services/`
```csharp
public class ProductService : IProductService
{
    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = new Product(request.Name, request.Sku);
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductDto>(product);
    }
}
```

4. **Add Controller Action**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
{
    var result = await _productService.CreateAsync(request);
    return Ok(new ApiResponse { Data = result });
}
```

5. **Register in DI Container** (Program.cs)
```csharp
services.AddScoped<IProductService, ProductService>();
```

---

## Summary

The WMS Clean Architecture ensures:
- ✅ **Testability** - Each layer independently testable
- ✅ **Maintainability** - Clear separation of concerns
- ✅ **Scalability** - Horizontal scaling per service
- ✅ **Flexibility** - Easy to add/modify features
- ✅ **Reusability** - Domain logic isolated
- ✅ **Performance** - Optimized caching and queries

**Status**: This architecture is production-ready and supports the Warehouse Management System's complex business requirements.
