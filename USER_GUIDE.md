# WMS (Warehouse Management System) - User Guide

## Table of Contents

1. [Overview](#overview)
2. [Technical Stack](#technical-stack)
3. [Project Architecture](#project-architecture)
4. [Getting Started](#getting-started)
5. [Configuration](#configuration)
6. [API Reference](#api-reference)
7. [Web Application Guide](#web-application-guide)
8. [User Roles & Permissions](#user-roles--permissions)
9. [Core Features](#core-features)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The **Warehouse Management System (WMS)** is a comprehensive solution for managing warehouse operations including:

- **Product Management** - SKU/product catalog management
- **Location Management** - Warehouse zones, aisles, racks, shelves, and bins
- **Inventory Management** - Real-time stock levels and transactions
- **Inbound Processing** - Receiving and put-away operations
- **Outbound Processing** - Picking, packing, and shipping
- **Payment Management** - Payment tracking for shipments
- **Delivery Management** - Delivery tracking and status updates

---

## Technical Stack

### Backend (WMS.API)

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 9.0 | Runtime framework |
| ASP.NET Core Web API | 9.0 | RESTful API framework |
| Entity Framework Core | 9.0.0 | ORM / Data access |
| SQL Server | LocalDB/Express | Database |
| JWT Bearer Authentication | 9.0.0 | Security & Authentication |
| Swashbuckle (Swagger) | 7.0.5 | API documentation |

### Frontend (WMS.Web)

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core MVC | 9.0 | Web application framework |
| Razor Views | - | Server-side rendering |
| Bootstrap | 5.x | UI framework |
| Newtonsoft.Json | 13.0.3 | JSON serialization |

### Project Structure

```
WMS/
??? WMS.API/                 # RESTful API project
?   ??? Controllers/         # API endpoints
?   ??? appsettings.json     # API configuration
?   ??? Program.cs           # Application entry point
?
??? WMS.Web/                 # Web MVC application
?   ??? Controllers/         # MVC controllers
?   ??? Views/               # Razor views
?   ??? Models/              # View models
?   ??? Services/            # API service clients
?   ??? appsettings.json     # Web configuration
?
??? WMS.Application/         # Application layer
?   ??? Interfaces/          # Service interfaces
?   ??? DTOs/                # Data transfer objects
?   ??? Common/              # Shared models
?
??? WMS.Domain/              # Domain layer
?   ??? Entities/            # Domain entities
?   ??? Enums/               # Enumerations
?   ??? Interfaces/          # Repository interfaces
?
??? WMS.Infrastructure/      # Infrastructure layer
    ??? Data/                # EF Core DbContext
    ??? Repositories/        # Repository implementations
    ??? Services/            # Service implementations
    ??? Migrations/          # Database migrations
```

---

## Project Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
???????????????????????????????????????????????????????????????
?                      Presentation Layer                      ?
?              (WMS.API / WMS.Web)                             ?
???????????????????????????????????????????????????????????????
?                      Application Layer                       ?
?              (WMS.Application)                               ?
?         - Service Interfaces                                 ?
?         - DTOs                                               ?
?         - Business Logic Contracts                           ?
???????????????????????????????????????????????????????????????
?                       Domain Layer                           ?
?              (WMS.Domain)                                    ?
?         - Entities                                           ?
?         - Enums                                              ?
?         - Repository Interfaces                              ?
???????????????????????????????????????????????????????????????
?                    Infrastructure Layer                      ?
?              (WMS.Infrastructure)                            ?
?         - EF Core DbContext                                  ?
?         - Repository Implementations                         ?
?         - Service Implementations                            ?
?         - External Service Integrations                      ?
???????????????????????????????????????????????????????????????
```

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WMS
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (see [Configuration](#configuration))

4. **Apply database migrations**
   ```bash
   cd WMS.Infrastructure
   dotnet ef database update --startup-project ../WMS.API
   ```

5. **Run the API**
   ```bash
   cd WMS.API
   dotnet run
   ```
   The API will be available at `https://localhost:5001` (Swagger UI at root)

6. **Run the Web Application** (in a separate terminal)
   ```bash
   cd WMS.Web
   dotnet run
   ```
   The web application will be available at `https://localhost:5000`

---

## Configuration

### API Configuration (WMS.API/appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
    "Issuer": "WMS.API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:5173", "http://localhost:3000", "http://localhost:5000" ]
  }
}
```

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JwtSettings:SecretKey` | JWT signing key (min 32 characters) |
| `JwtSettings:Issuer` | JWT token issuer |
| `JwtSettings:Audience` | JWT token audience |
| `JwtSettings:ExpirationMinutes` | Access token validity period |
| `JwtSettings:RefreshTokenExpirationDays` | Refresh token validity period |
| `Cors:AllowedOrigins` | Allowed CORS origins |

### Web Configuration (WMS.Web/appsettings.json)

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

| Setting | Description |
|---------|-------------|
| `ApiSettings:BaseUrl` | Base URL of the WMS API |

---

## API Reference

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | User login | No |
| POST | `/api/auth/register` | User registration | No |
| POST | `/api/auth/refresh-token` | Refresh access token | No |
| POST | `/api/auth/logout` | User logout | Yes |
| GET | `/api/auth/profile` | Get user profile | Yes |
| GET | `/api/auth/validate` | Validate token | Yes |

#### Login Request
```json
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

#### Login Response
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresAt": "2024-01-15T12:00:00Z",
    "user": {
      "id": "guid",
      "username": "admin",
      "email": "admin@wms.com",
      "firstName": "System",
      "lastName": "Administrator",
      "roles": ["Admin"]
    }
  }
}
```

### Product Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/products` | Get all products (paginated) | Any authenticated |
| GET | `/api/products/{id}` | Get product by ID | Any authenticated |
| GET | `/api/products/sku/{sku}` | Get product by SKU | Any authenticated |
| POST | `/api/products` | Create product | Admin, Manager |
| PUT | `/api/products/{id}` | Update product | Admin, Manager |
| PATCH | `/api/products/{id}/activate` | Activate product | Admin, Manager |
| PATCH | `/api/products/{id}/deactivate` | Deactivate product | Admin, Manager |

#### Create Product Request
```json
POST /api/products
{
  "sku": "PROD-001",
  "name": "Product Name",
  "description": "Product description",
  "uom": "PCS",
  "weight": 1.5,
  "length": 10,
  "width": 5,
  "height": 2,
  "barcode": "1234567890123",
  "category": "Electronics",
  "reorderLevel": 10,
  "maxStockLevel": 100
}
```

### Location Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/location` | Get all locations (paginated) | Any authenticated |
| GET | `/api/location/{id}` | Get location by ID | Any authenticated |
| POST | `/api/location` | Create location | Admin, Manager |
| PUT | `/api/location/{id}` | Update location | Admin, Manager |
| PATCH | `/api/location/{id}/activate` | Activate location | Admin, Manager |
| PATCH | `/api/location/{id}/deactivate` | Deactivate location | Admin, Manager |

### Inventory Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/inventory` | Get all inventory records | Admin, Manager, WarehouseStaff |
| GET | `/api/inventory/{id}` | Get inventory by ID | Admin, Manager, WarehouseStaff |
| GET | `/api/inventory/product/{productId}` | Get inventory by product | Admin, Manager, WarehouseStaff |
| GET | `/api/inventory/levels` | Get inventory levels | Admin, Manager, WarehouseStaff |
| GET | `/api/inventory/transactions` | Get inventory transactions | Admin, Manager, WarehouseStaff |
| GET | `/api/inventory/availability` | Check stock availability | Admin, Manager, WarehouseStaff |

### Inbound Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/inbound` | Get all inbound shipments | Admin, Manager, WarehouseStaff |
| GET | `/api/inbound/{id}` | Get inbound by ID | Admin, Manager, WarehouseStaff |
| POST | `/api/inbound` | Create inbound shipment | Admin, Manager, WarehouseStaff |
| POST | `/api/inbound/{id}/receive` | Receive inbound items | Admin, Manager, WarehouseStaff |
| POST | `/api/inbound/{id}/cancel` | Cancel inbound | Admin, Manager |
| GET | `/api/inbound/statistics` | Get inbound statistics | Admin, Manager |

#### Create Inbound Request
```json
POST /api/inbound
{
  "referenceNumber": "PO-2024-001",
  "supplierName": "Supplier ABC",
  "supplierCode": "SUP001",
  "expectedDate": "2024-01-20T00:00:00Z",
  "notes": "Urgent delivery",
  "items": [
    {
      "productId": "product-guid",
      "locationId": "location-guid",
      "expectedQuantity": 100,
      "lotNumber": "LOT001",
      "expiryDate": "2025-01-20T00:00:00Z"
    }
  ]
}
```

### Outbound Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/outbound` | Get all outbound orders | Admin, Manager, WarehouseStaff |
| GET | `/api/outbound/{id}` | Get outbound by ID | Admin, Manager, WarehouseStaff |
| POST | `/api/outbound` | Create outbound order | Admin, Manager, WarehouseStaff |
| POST | `/api/outbound/{id}/pick` | Pick items | Admin, Manager, WarehouseStaff |
| POST | `/api/outbound/{id}/ship` | Ship order | Admin, Manager, WarehouseStaff |
| POST | `/api/outbound/{id}/cancel` | Cancel order | Admin, Manager |
| GET | `/api/outbound/statistics` | Get outbound statistics | Admin, Manager |

### Payment Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/payment` | Get all payments | Admin, Manager |
| GET | `/api/payment/{id}` | Get payment by ID | Admin, Manager |
| POST | `/api/payment` | Create payment | Admin, Manager |
| POST | `/api/payment/{id}/initiate` | Initiate payment | Admin, Manager |
| POST | `/api/payment/{id}/confirm` | Confirm payment | Admin, Manager |
| POST | `/api/payment/webhook` | Process payment webhook | No (system) |
| GET | `/api/payment/can-ship/{outboundId}` | Check if can ship | Admin, Manager, WarehouseStaff |

### Delivery Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/delivery` | Get all deliveries | Admin, Manager, WarehouseStaff |
| GET | `/api/delivery/{id}` | Get delivery by ID | Admin, Manager, WarehouseStaff |
| POST | `/api/delivery` | Create delivery | Admin, Manager |
| PATCH | `/api/delivery/{id}/status` | Update delivery status | Admin, Manager, WarehouseStaff |
| GET | `/api/delivery/track/{trackingNumber}` | Track delivery | No (public) |

---

## Web Application Guide

### Login

1. Navigate to the web application URL
2. Enter your credentials:
   - **Default Admin**: `admin` / `Admin@123`
3. Click "Login"

### Dashboard

The dashboard provides an overview of:
- Total products, locations, and inventory items
- Recent inbound/outbound activities
- Quick access to common operations

### Managing Products

1. Navigate to **Products** from the menu
2. **View Products**: Browse paginated list with search
3. **Add Product**: Click "Add Product", fill the form
4. **Edit Product**: Click the edit icon on any product row
5. **Activate/Deactivate**: Toggle product status

### Managing Locations

1. Navigate to **Locations** from the menu
2. Locations are organized hierarchically (Zone ? Aisle ? Rack ? Shelf ? Bin)
3. Set capacity for each location to track occupancy

### Inbound Operations

1. **Create Inbound Order**:
   - Navigate to **Inbound** ? **New Inbound**
   - Enter supplier details and expected date
   - Add items with products, locations, and quantities
   
2. **Receive Goods**:
   - Open the inbound order
   - Click "Receive"
   - Enter received quantities (and any damaged quantities)
   - System automatically updates inventory

3. **Complete/Cancel**:
   - Mark as complete when all items received
   - Cancel if order is no longer needed

### Outbound Operations

1. **Create Outbound Order**:
   - Navigate to **Outbound** ? **New Outbound**
   - Enter customer details and shipping address
   - Add items with products, locations, and quantities
   - System validates stock availability

2. **Pick Items**:
   - Open the outbound order
   - Click "Start Picking"
   - Confirm picked quantities per item

3. **Ship Order**:
   - After picking is complete, click "Ship"
   - System deducts inventory and creates delivery record

### Inventory Management

1. **View Inventory Levels**:
   - Navigate to **Inventory** ? **Levels**
   - See real-time stock by product and location

2. **View Transactions**:
   - Navigate to **Inventory** ? **Transactions**
   - Filter by product, location, or date range

### Payment & Delivery

1. **Payments** are linked to outbound orders
2. **Delivery tracking** shows shipment status and events

---

## User Roles & Permissions

### Role Hierarchy

| Role | Description | Permissions |
|------|-------------|-------------|
| **Admin** | System administrator | Full access to all features |
| **Manager** | Warehouse manager | Manage products, locations, view reports, approve operations |
| **WarehouseStaff** | Floor worker | Execute inbound/outbound operations, view inventory |

### Permission Matrix

| Feature | Admin | Manager | WarehouseStaff |
|---------|-------|---------|----------------|
| View Products | ? | ? | ? |
| Create/Edit Products | ? | ? | ? |
| View Locations | ? | ? | ? |
| Create/Edit Locations | ? | ? | ? |
| View Inventory | ? | ? | ? |
| Create Inbound | ? | ? | ? |
| Receive Inbound | ? | ? | ? |
| Cancel Inbound | ? | ? | ? |
| Create Outbound | ? | ? | ? |
| Pick/Ship Outbound | ? | ? | ? |
| Cancel Outbound | ? | ? | ? |
| Manage Payments | ? | ? | ? |
| View Reports | ? | ? | ? |
| User Management | ? | ? | ? |

---

## Core Features

### Status Workflows

#### Inbound Status Flow
```
Pending ? Received ? PutAway ? Completed
    ?
 Cancelled
```

| Status | Description |
|--------|-------------|
| Pending | Order created, awaiting goods |
| Received | Goods received, pending put-away |
| PutAway | Items stored in locations |
| Completed | All operations finished |
| Cancelled | Order cancelled |

#### Outbound Status Flow
```
Pending ? Picking ? Picked ? Packed ? Shipped
    ?
 Cancelled
```

| Status | Description |
|--------|-------------|
| Pending | Order created, awaiting processing |
| Picking | Items being picked from locations |
| Picked | All items picked |
| Packed | Items packed for shipment |
| Shipped | Order shipped (inventory deducted) |
| Cancelled | Order cancelled |

#### Payment Status Flow
```
Pending ? Confirmed
    ?
 Failed/Cancelled
```

| Status | Description |
|--------|-------------|
| Pending | Payment initiated |
| Confirmed | Payment received |
| Failed | Payment failed |
| Cancelled | Payment cancelled |

#### Delivery Status Flow
```
Pending ? InTransit ? Delivered
    ?         ?
 Cancelled  Failed ? Returned
```

| Status | Description |
|--------|-------------|
| Pending | Delivery created |
| InTransit | Package in transit |
| Delivered | Successfully delivered |
| Failed | Delivery failed |
| Returned | Package returned |
| Cancelled | Delivery cancelled |

### Payment Types

| Type | Description | Shipping Rule |
|------|-------------|---------------|
| **Prepaid** | Payment before shipping | Must be Confirmed before ship |
| **COD** | Cash on Delivery | Can ship without payment confirmation |
| **Postpaid** | Payment after delivery | Can ship without payment confirmation |

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Failed
```
Error: Cannot connect to SQL Server
```
**Solution**:
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists or migrations are applied

#### 2. JWT Token Expired
```
Error: 401 Unauthorized
```
**Solution**:
- Use the refresh token endpoint to get a new access token
- Re-login if refresh token is also expired

#### 3. CORS Error
```
Error: Access-Control-Allow-Origin blocked
```
**Solution**:
- Add the client URL to `Cors:AllowedOrigins` in API config

#### 4. Insufficient Stock
```
Error: Insufficient quantity for product X
```
**Solution**:
- Check inventory levels before creating outbound
- Process pending inbound orders first

#### 5. Swagger UI Not Loading
```
Error: ReflectionTypeLoadException
```
**Solution**:
- Ensure Swashbuckle.AspNetCore version is 7.0.5 (compatible with .NET 9)
- Clean and rebuild the solution

### Logging

Logs are configured in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Support

For additional support:
1. Check the Swagger documentation at `/swagger`
2. Review API response messages for specific errors
3. Check application logs for detailed error information

---

## Database Schema

### Core Entities

| Entity | Description |
|--------|-------------|
| Product | Product/SKU master data |
| Location | Warehouse location hierarchy |
| Inventory | Current stock levels by product/location |
| InventoryTransaction | Stock movement audit trail |
| Inbound | Receiving orders |
| InboundItem | Receiving order line items |
| Outbound | Shipping orders |
| OutboundItem | Shipping order line items |
| Payment | Payment records linked to outbounds |
| PaymentEvent | Payment audit trail |
| Delivery | Delivery/shipment records |
| DeliveryEvent | Delivery tracking events |
| User | System users |
| Role | User roles |
| UserRole | User-role assignments |

### Entity Relationships

```
Product ???????????? Inventory ???????????? Location
    ?                    ?                      ?
InventoryTransaction  InboundItem         OutboundItem
                          ?                     ?
                       Inbound              Outbound
                                               ?
                                      ???????????????????
                                      ?                 ?
                                   Payment          Delivery
                                      ?                 ?
                                PaymentEvent     DeliveryEvent
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024-01 | Initial release |

---

*This documentation was generated for WMS v1.0.0 targeting .NET 9.0*
