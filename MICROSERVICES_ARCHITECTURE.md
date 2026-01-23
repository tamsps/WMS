# WMS Microservices Architecture

## Overview

This document describes the refactored WMS (Warehouse Management System) microservices architecture. The monolithic WMS.API has been decomposed into 8 independent microservices, each responsible for a specific business domain.

## Microservices Architecture

```
???????????????????????????????????????????????????????????????????????
?                         API Gateway (Future)                         ?
?                    (Ocelot / YARP / Custom)                         ?
???????????????????????????????????????????????????????????????????????
               ?
    ???????????????????????????????????????????????????????????
    ?                                                           ?
    ???? WMS.Auth.API         (Port: 5001) ????????????????????
    ???? WMS.Products.API     (Port: 5002) ????????????????????
    ???? WMS.Locations.API    (Port: 5003) ????????????????????
    ???? WMS.Inventory.API    (Port: 5004) ????????????????????
    ???? WMS.Inbound.API      (Port: 5005) ????????????????????
    ???? WMS.Outbound.API     (Port: 5006) ????????????????????
    ???? WMS.Payment.API      (Port: 5007) ????????????????????
    ???? WMS.Delivery.API     (Port: 5008) ????????????????????
               ?
    ???????????????????????????????????????????????????????????
    ?                    Shared Database                       ?
    ?              (SQL Server - WMSDB)                        ?
    ?     OR Individual Databases per Service (recommended)   ?
    ????????????????????????????????????????????????????????????
```

## Microservices Details

### 1. WMS.Auth.API (Authentication Service)
**Port:** 5001  
**Responsibility:** User authentication, authorization, JWT token management  
**Endpoints:**
- POST /api/auth/login
- POST /api/auth/register
- POST /api/auth/refresh-token
- POST /api/auth/logout
- GET /api/auth/profile
- GET /api/auth/validate

**Dependencies:**
- WMS.Domain (User, Role entities)
- WMS.Application (IAuthService, ITokenService)
- WMS.Infrastructure (AuthService, TokenService)

---

### 2. WMS.Products.API (Product Management Service)
**Port:** 5002  
**Responsibility:** Product catalog management, SKU operations  
**Endpoints:**
- GET /api/products (with pagination)
- GET /api/products/{id}
- GET /api/products/sku/{sku}
- POST /api/products
- PUT /api/products/{id}
- PATCH /api/products/{id}/activate
- PATCH /api/products/{id}/deactivate

**Dependencies:**
- WMS.Domain (Product entity)
- WMS.Application (IProductService)
- WMS.Infrastructure (ProductService)

---

### 3. WMS.Locations.API (Location Management Service)
**Port:** 5003  
**Responsibility:** Warehouse location hierarchy management  
**Endpoints:**
- GET /api/location (with pagination)
- GET /api/location/{id}
- POST /api/location
- PUT /api/location/{id}
- PATCH /api/location/{id}/activate
- PATCH /api/location/{id}/deactivate

**Dependencies:**
- WMS.Domain (Location entity)
- WMS.Application (ILocationService)
- WMS.Infrastructure (LocationService)

---

### 4. WMS.Inventory.API (Inventory Management Service)
**Port:** 5004  
**Responsibility:** Stock level tracking, inventory transactions  
**Endpoints:**
- GET /api/inventory (all inventory records)
- GET /api/inventory/{id}
- GET /api/inventory/product/{productId}
- GET /api/inventory/levels
- GET /api/inventory/transactions
- GET /api/inventory/availability

**Dependencies:**
- WMS.Domain (Inventory, InventoryTransaction entities)
- WMS.Application (IInventoryService)
- WMS.Infrastructure (InventoryService)
- **External Calls:** Products.API, Locations.API (for validation)

---

### 5. WMS.Inbound.API (Inbound Operations Service)
**Port:** 5005  
**Responsibility:** Receiving operations, put-away processing  
**Endpoints:**
- GET /api/inbound (with pagination)
- GET /api/inbound/{id}
- POST /api/inbound
- POST /api/inbound/{id}/receive
- POST /api/inbound/{id}/cancel
- GET /api/inbound/statistics

**Dependencies:**
- WMS.Domain (Inbound, InboundItem entities)
- WMS.Application (IInboundService, IInventoryService)
- WMS.Infrastructure (InboundService, InventoryService)
- **External Calls:** Products.API, Locations.API, Inventory.API

---

### 6. WMS.Outbound.API (Outbound Operations Service)
**Port:** 5006  
**Responsibility:** Picking, packing, shipping operations  
**Endpoints:**
- GET /api/outbound (with pagination)
- GET /api/outbound/{id}
- POST /api/outbound
- POST /api/outbound/{id}/pick
- POST /api/outbound/{id}/ship
- POST /api/outbound/{id}/cancel
- GET /api/outbound/statistics

**Dependencies:**
- WMS.Domain (Outbound, OutboundItem entities)
- WMS.Application (IOutboundService, IInventoryService)
- WMS.Infrastructure (OutboundService, InventoryService)
- **External Calls:** Products.API, Locations.API, Inventory.API, Payment.API

---

### 7. WMS.Payment.API (Payment Management Service)
**Port:** 5007  
**Responsibility:** Payment tracking, shipment gating  
**Endpoints:**
- GET /api/payment (with pagination)
- GET /api/payment/{id}
- POST /api/payment
- POST /api/payment/{id}/initiate
- POST /api/payment/{id}/confirm
- POST /api/payment/webhook
- GET /api/payment/can-ship/{outboundId}

**Dependencies:**
- WMS.Domain (Payment, PaymentEvent entities)
- WMS.Application (IPaymentService)
- WMS.Infrastructure (PaymentService)
- **External Calls:** Outbound.API (for validation)

---

### 8. WMS.Delivery.API (Delivery Management Service)
**Port:** 5008  
**Responsibility:** Delivery tracking, shipment status updates  
**Endpoints:**
- GET /api/delivery (with pagination)
- GET /api/delivery/{id}
- POST /api/delivery
- PATCH /api/delivery/{id}/status
- GET /api/delivery/track/{trackingNumber}
- POST /api/delivery/{id}/events

**Dependencies:**
- WMS.Domain (Delivery, DeliveryEvent entities)
- WMS.Application (IDeliveryService)
- WMS.Infrastructure (DeliveryService)
- **External Calls:** Outbound.API (for validation)

---

## Project Structure

Each microservice follows the same structure:

```
WMS.{ServiceName}.API/
??? Controllers/
?   ??? {ServiceName}Controller.cs
??? appsettings.json
??? appsettings.Development.json
??? Program.cs
??? WMS.{ServiceName}.API.csproj
??? Properties/
    ??? launchSettings.json
```

**Shared Projects** (referenced by all microservices):
- **WMS.Domain** - Domain entities and interfaces
- **WMS.Application** - Application DTOs and service interfaces
- **WMS.Infrastructure** - Service implementations and data access

---

## Database Strategy

### Option 1: Shared Database (Current - Simpler)
- All microservices connect to the same WMSDB
- Easier for development and testing
- Transactions can span multiple entities
- **Concern:** Tight coupling at the database level

### Option 2: Database per Service (Recommended for Production)
- Each microservice has its own database schema/instance
- True service autonomy
- Requires distributed transactions or saga patterns
- Use event-driven communication for cross-service operations

**Current Implementation:** Shared database for simplicity

---

## Inter-Service Communication

### Synchronous (HTTP REST)
- Used for immediate validation and queries
- Example: Outbound.API calls Payment.API to check if shipment can proceed

### Asynchronous (Message Queue - Future Enhancement)
- Use RabbitMQ, Azure Service Bus, or Kafka
- Example: When inventory changes, publish event for interested services

---

## Authentication & Authorization

All services (except Auth.API endpoints) require JWT authentication:

1. Client calls **Auth.API** to get JWT token
2. Client includes JWT token in subsequent requests to other services
3. Each service validates the JWT token independently
4. Services share the same JWT secret key for validation

**Shared Configuration:**
```json
"JwtSettings": {
  "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
  "Issuer": "WMS.Microservices",
  "Audience": "WMS.Client",
  "ExpirationMinutes": 60
}
```

---

## Running the Microservices

### Development Mode

Run each service individually in separate terminals:

```bash
# Terminal 1 - Auth Service
cd WMS.Auth.API
dotnet run --urls=https://localhost:5001

# Terminal 2 - Products Service
cd WMS.Products.API
dotnet run --urls=https://localhost:5002

# Terminal 3 - Locations Service
cd WMS.Locations.API
dotnet run --urls=https://localhost:5003

# ... and so on for other services
```

### Docker Compose (Recommended)

Use `docker-compose.yml` to run all services:

```bash
docker-compose up
```

---

## Configuration Files

Each service has its own `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
    "Issuer": "WMS.Microservices",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60
  },
  "ServiceUrls": {
    "AuthService": "https://localhost:5001",
    "ProductsService": "https://localhost:5002",
    "LocationsService": "https://localhost:5003",
    "InventoryService": "https://localhost:5004",
    "InboundService": "https://localhost:5005",
    "OutboundService": "https://localhost:5006",
    "PaymentService": "https://localhost:5007",
    "DeliveryService": "https://localhost:5008"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5000",
      "https://localhost:5000"
    ]
  }
}
```

---

## Migration from Monolith

### Steps Completed:
1. ? Analyzed monolithic WMS.API controllers
2. ? Identified bounded contexts (8 microservices)
3. ? Created microservice project structure
4. ? Moved controllers to respective microservices
5. ? Configured individual ports and settings
6. ? Updated WMS.Web to call distributed services

### Pending Enhancements:
- [ ] API Gateway implementation (Ocelot/YARP)
- [ ] Service discovery (Consul/Eureka)
- [ ] Circuit breaker pattern (Polly)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Centralized logging (ELK/Seq)
- [ ] Message queue integration
- [ ] Database per service migration
- [ ] Health checks and monitoring
- [ ] Docker containerization
- [ ] Kubernetes orchestration

---

## Testing

Each microservice can be tested independently:

### Swagger UI URLs:
- Auth: https://localhost:5001/swagger
- Products: https://localhost:5002/swagger
- Locations: https://localhost:5003/swagger
- Inventory: https://localhost:5004/swagger
- Inbound: https://localhost:5005/swagger
- Outbound: https://localhost:5006/swagger
- Payment: https://localhost:5007/swagger
- Delivery: https://localhost:5008/swagger

---

## Benefits of Microservices Architecture

1. **Independent Deployment** - Update one service without redeploying others
2. **Technology Diversity** - Different services can use different tech stacks
3. **Scalability** - Scale high-traffic services independently
4. **Team Autonomy** - Different teams own different services
5. **Fault Isolation** - Failure in one service doesn't crash the system
6. **Development Speed** - Smaller codebases, faster development cycles

---

## Challenges & Solutions

| Challenge | Solution |
|-----------|----------|
| Data Consistency | Eventual consistency, Saga pattern |
| Distributed Transactions | Use compensating transactions |
| Service Discovery | Use Consul or built-in Kubernetes DNS |
| Network Latency | Cache frequently accessed data |
| Debugging Complexity | Distributed tracing, centralized logging |
| Testing | Contract testing, integration tests |

---

## Next Steps

1. Implement API Gateway for unified entry point
2. Add health check endpoints to all services
3. Implement distributed caching (Redis)
4. Add message queue for async communication
5. Create CI/CD pipelines per service
6. Implement monitoring and alerting
7. Document service contracts (OpenAPI)
8. Perform load testing and optimization

---

*Last Updated: January 2024*  
*Architecture Version: 1.0*
