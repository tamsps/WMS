# WMS (Warehouse Management System) - Complete Architecture & Deployment Guide

**Version**: 1.0  
**Date**: January 28, 2026  
**Status**: Production Ready

---

## Table of Contents

1. [System Architecture Overview](#system-architecture-overview)
2. [Service Responsibilities](#service-responsibilities)
3. [API Specifications](#api-specifications)
4. [Database Deployment](#database-deployment)
5. [Service Startup Guide](#service-startup-guide)
6. [Batch Scripts for Service Management](#batch-scripts-for-service-management)
7. [Configuration & Environment Setup](#configuration--environment-setup)
8. [Troubleshooting](#troubleshooting)

---

## System Architecture Overview

### Architecture Pattern: Clean Architecture + Microservices + API Gateway

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT APPLICATIONS                         │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  Web UI (MVC)      Mobile App (Flutter)   Admin Dashboard   │   │
│  │  Port: 5001        Port: 5008            Port: TBD          │   │
│  └──────────────────────────────────────────────────────────────┘   │
└────────────────────────────┬────────────────────────────────────────┘
                             │ HTTP(S) Requests
                             ▼
         ┌───────────────────────────────────────────┐
         │     API GATEWAY (YARP Reverse Proxy)     │
         │          Port: 5000                       │
         │  • Request routing                        │
         │  • Authentication/Authorization           │
         │  • Rate limiting                          │
         │  • Load balancing                         │
         └───────────────────┬───────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
   ┌─────────────┐  ┌──────────────┐  ┌──────────────┐
   │   Auth API  │  │ Product API  │  │ Location API │
   │  Port:5002  │  │  Port: 5003  │  │  Port: 5004  │
   └─────────────┘  └──────────────┘  └──────────────┘
        ▼                    ▼                    ▼
   ┌─────────────┐  ┌──────────────┐  ┌──────────────┐
   │ Monolithic  │  │ Microservice │  │ Microservice │
   │   Service   │  │   Service    │  │   Service    │
   └─────────────┘  └──────────────┘  └──────────────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
   ┌─────────────┐  ┌──────────────┐  ┌──────────────┐
   │ Inbound API │  │ Outbound API │  │ Payment API  │
   │  Port: 5005 │  │  Port: 5006  │  │  Port: 5007  │
   └─────────────┘  └──────────────┘  └──────────────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
   ┌─────────────┐  ┌──────────────┐  ┌──────────────┐
   │ Delivery    │  │ Inventory    │  │    WMS      │
   │ Microservice│  │ Microservice │  │  Main API   │
   │  Port: 5009 │  │  Port: 5010  │  │  Port: 5011 │
   └─────────────┘  └──────────────┘  └──────────────┘
        │                    │                    │
        └────────────────────┴────────────────────┘
                             │
        ┌────────────────────────────────────────┐
        ▼                                        ▼
   ┌─────────────────────────────────────────────────────┐
   │             SQL Server LocalDB (WMSDB)              │
   │  • Unified database for all services               │
   │  • Entity Framework Core migrations                │
   │  • Real-time data synchronization                  │
   └─────────────────────────────────────────────────────┘
```

### Technology Stack

```
Framework         : .NET 9 (C# 12)
Web Framework     : ASP.NET Core MVC
API Pattern       : REST with Clean Architecture
Authentication   : JWT (JSON Web Tokens)
Authorization    : Role-based (RBAC)
API Gateway      : YARP (Yet Another Reverse Proxy)
Database         : SQL Server 2019+ / LocalDB
ORM              : Entity Framework Core 9.0
Frontend         : Bootstrap 5.3 + Razor Views
Mobile           : Flutter (optional)
Caching          : Distributed Memory Cache / Redis (optional)
Logging          : Serilog (optional)
Testing          : xUnit / NUnit
```

### Project Structure

```
WMS/
├── WMS.sln (Solution File)
│
├── Core Projects (Shared)
│   ├── WMS.Domain/                 # Domain models, entities, interfaces
│   ├── WMS.Application/             # DTOs, service interfaces
│   └── WMS.Infrastructure/          # EF Core, repositories, migrations
│
├── Monolithic API
│   ├── WMS.API/                     # All-in-one API (Auth, Inventory, Locations)
│   │   ├── Controllers/             # API endpoints
│   │   ├── appsettings.json        # Configuration
│   │   └── Program.cs              # Service registration
│
├── Microservices (Individual)
│   ├── WMS.Auth.API/                # Authentication & Authorization
│   │   ├── WMS.Auth.Application/   # Business logic
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Products.API/            # Product Management
│   │   ├── WMS.Products.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Locations.API/           # Location Management
│   │   ├── WMS.Locations.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Inbound.API/             # Inbound Operations
│   │   ├── WMS.Inbound.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Outbound.API/            # Outbound Operations
│   │   ├── WMS.Outbound.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Inventory.API/           # Inventory Management
│   │   ├── WMS.Inventory.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   ├── WMS.Payment.API/             # Payment Processing
│   │   ├── WMS.Payment.Application/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   │
│   └── WMS.Delivery.API/            # Delivery Tracking
│       ├── WMS.Delivery.Application/
│       ├── Controllers/
│       └── appsettings.json
│
├── API Gateway
│   └── WMS.Gateway/                 # YARP Reverse Proxy
│       ├── appsettings.json        # Route configuration
│       └── Program.cs              # Gateway setup
│
├── Client Applications
│   ├── WMS.Web/                     # ASP.NET MVC Web UI
│   │   ├── Views/                   # Razor views (7 modules)
│   │   ├── Controllers/             # MVC controllers
│   │   ├── Models/                  # ViewModels
│   │   ├── Services/                # ApiService (HTTP client)
│   │   └── wwwroot/                 # Static files
│   │
│   └── WMS.Mobile/                  # Flutter Mobile App (optional)
│       └── lib/                     # Dart code
│
├── Testing
│   └── WMS.Tests/                   # xUnit test suite
│
└── Scripts & Documentation
    ├── run-all-services.ps1         # PowerShell: Start all services
    ├── apply-migrations.ps1         # PowerShell: Apply DB migrations
    ├── setup-database.ps1           # PowerShell: Initialize database
    ├── START_ALL_SERVICES.bat       # Batch: Windows script to run all
    └── *.md                         # Documentation files
```

---

## Service Responsibilities

### 1. API Gateway (WMS.Gateway) - Port 5000
**Purpose**: Central entry point for all client requests

**Responsibilities**:
- Route HTTP requests to appropriate microservices
- Uniform API endpoint for web and mobile clients
- Load balancing and failover
- API documentation aggregation
- CORS management

**Technologies**:
- YARP (Yet Another Reverse Proxy)
- Swagger/OpenAPI

**Routes Configuration** (appsettings.json):
```
/api/auth/*      → WMS.Auth.API:5002
/api/products/*  → WMS.Products.API:5003
/api/locations/* → WMS.Locations.API:5004
/api/inbound/*   → WMS.Inbound.API:5005
/api/outbound/*  → WMS.Outbound.API:5006
/api/inventory/* → WMS.Inventory.API:5010
/api/payment/*   → WMS.Payment.API:5007
/api/delivery/*  → WMS.Delivery.API:5009
/api/*           → WMS.API:5011 (fallback)
```

---

### 2. Authentication API (WMS.Auth.API) - Port 5002
**Purpose**: User authentication and JWT token generation

**Responsibilities**:
- User login/logout
- JWT token generation and validation
- Token refresh
- User profile management
- Role management (Admin, Manager, User, Viewer)

**Key Endpoints**:
```
POST   /api/auth/login              - User login
POST   /api/auth/logout             - User logout
POST   /api/auth/refresh            - Refresh token
GET    /api/auth/me                 - Get current user
POST   /api/auth/change-password    - Change password
GET    /api/auth/roles              - List available roles
```

**Related Projects**:
- WMS.Auth.API (API)
- WMS.Auth.Application (Business logic)

---

### 3. Product Service (WMS.Products.API) - Port 5003
**Purpose**: Product catalog management

**Responsibilities**:
- Create, read, update, delete products
- Product categorization
- SKU management
- Product activation/deactivation
- Bulk product operations

**Key Endpoints**:
```
GET    /api/products                - List all products (with pagination)
GET    /api/products/{id}           - Get product details
POST   /api/products                - Create new product
PUT    /api/products/{id}           - Update product
DELETE /api/products/{id}           - Delete product
PATCH  /api/products/{id}/activate  - Activate product
PATCH  /api/products/{id}/deactivate - Deactivate product
GET    /api/products/search         - Search products by name/SKU
```

**Related Projects**:
- WMS.Products.API (API)
- WMS.Products.Application (Business logic)

---

### 4. Location Service (WMS.Locations.API) - Port 5004
**Purpose**: Warehouse location management

**Responsibilities**:
- Define warehouse zones and locations
- Hierarchical location structure
- Storage capacity management
- Location type management (Zone, Rack, Bin, Shelf)
- Inventory tracking per location

**Key Endpoints**:
```
GET    /api/locations                - List all locations
GET    /api/locations/{id}           - Get location details
POST   /api/locations                - Create new location
PUT    /api/locations/{id}           - Update location
DELETE /api/locations/{id}           - Delete location
GET    /api/locations/{id}/inventory - Get location inventory
GET    /api/locations/tree           - Get location hierarchy
PUT    /api/locations/{id}/capacity  - Update location capacity
```

**Related Projects**:
- WMS.Locations.API (API)
- WMS.Locations.Application (Business logic)

---

### 5. Inbound Service (WMS.Inbound.API) - Port 5005
**Purpose**: Receiving and goods inflow operations

**Responsibilities**:
- Create inbound receiving orders
- Track received quantities
- Quality check validation
- Update inventory on receipt
- Multi-step receiving workflow
- Supplier management

**Key Endpoints**:
```
GET    /api/inbound                  - List inbound orders
GET    /api/inbound/{id}             - Get order details
POST   /api/inbound                  - Create inbound order
PUT    /api/inbound/{id}             - Update inbound order
DELETE /api/inbound/{id}             - Cancel inbound order
POST   /api/inbound/{id}/receive     - Start receiving items
PUT    /api/inbound/{id}/items/{itemId} - Update received quantity
POST   /api/inbound/{id}/complete    - Complete receiving
GET    /api/inbound/{id}/status      - Get receiving progress
```

**Workflow**:
```
Draft → Receiving → Completed
          ↓
      Cancelled
```

**Related Projects**:
- WMS.Inbound.API (API)
- WMS.Inbound.Application (Business logic)

---

### 6. Outbound Service (WMS.Outbound.API) - Port 5006
**Purpose**: Order fulfillment and shipping operations

**Responsibilities**:
- Create customer orders
- Pick items from inventory
- Pack orders
- Generate shipping labels
- Track shipment status
- Multi-step fulfillment workflow

**Key Endpoints**:
```
GET    /api/outbound                 - List outbound orders
GET    /api/outbound/{id}            - Get order details
POST   /api/outbound                 - Create outbound order
PUT    /api/outbound/{id}            - Update order
DELETE /api/outbound/{id}            - Cancel order
POST   /api/outbound/{id}/pick       - Pick items
PUT    /api/outbound/{id}/items/{itemId} - Update pick quantity
POST   /api/outbound/{id}/ship       - Ship order
GET    /api/outbound/{id}/status     - Get fulfillment progress
GET    /api/outbound/search          - Search by customer/reference
```

**Workflow**:
```
Draft → Picking → Picked → Shipped
  ↓
Cancelled
```

**Related Projects**:
- WMS.Outbound.API (API)
- WMS.Outbound.Application (Business logic)

---

### 7. Payment Service (WMS.Payment.API) - Port 5007
**Purpose**: Financial transaction management

**Responsibilities**:
- Process payments for inbound/outbound operations
- Support multiple payment methods
- Multi-currency transactions
- Payment verification
- Transaction history tracking
- Refund processing

**Key Endpoints**:
```
GET    /api/payment                  - List payments
GET    /api/payment/{id}             - Get payment details
POST   /api/payment                  - Create payment
PUT    /api/payment/{id}             - Update payment
DELETE /api/payment/{id}             - Cancel payment
POST   /api/payment/{id}/confirm     - Confirm payment
GET    /api/payment/search           - Search payments
POST   /api/payment/{id}/refund      - Process refund
```

**Supported Payment Methods**:
- Cash
- Credit Card
- Bank Transfer
- Digital Wallet
- Check

**Related Projects**:
- WMS.Payment.API (API)
- WMS.Payment.Application (Business logic)

---

### 8. Delivery Service (WMS.Delivery.API) - Port 5009
**Purpose**: Shipment tracking and delivery management

**Responsibilities**:
- Track deliveries in real-time
- Update delivery status
- Manage carrier information
- Customer delivery notifications
- Delivery timeline tracking
- Public tracking interface

**Key Endpoints**:
```
GET    /api/delivery                 - List deliveries (admin)
GET    /api/delivery/{id}            - Get delivery details (admin)
POST   /api/delivery                 - Create delivery
PUT    /api/delivery/{id}            - Update delivery
DELETE /api/delivery/{id}            - Cancel delivery
PUT    /api/delivery/{id}/status     - Update delivery status
GET    /api/delivery/track/{trackingId} - Public tracking (no auth)
POST   /api/delivery/{id}/notify     - Send customer notification
```

**Supported Carriers**:
- FedEx
- UPS
- DHL
- USPS
- Local Courier

**Delivery Status**:
```
Pending → In Transit → Out for Delivery → Delivered
            ↓
        Failed / Returned
```

**Related Projects**:
- WMS.Delivery.API (API)
- WMS.Delivery.Application (Business logic)

---

### 9. Inventory Service (WMS.Inventory.API) - Port 5010
**Purpose**: Real-time inventory management and tracking

**Responsibilities**:
- Track stock levels across locations
- Monitor stock movements
- Generate inventory reports
- Low stock alerts
- Inventory reconciliation
- Transaction audit trail

**Key Endpoints**:
```
GET    /api/inventory                - Get inventory overview
GET    /api/inventory/{productId}    - Get product inventory
GET    /api/inventory/location/{locationId} - Get location inventory
GET    /api/inventory/movements      - Get stock movements
POST   /api/inventory/adjust         - Manual inventory adjustment
GET    /api/inventory/alerts         - Get low stock alerts
GET    /api/inventory/transactions   - Get transaction history
POST   /api/inventory/reconcile      - Reconcile inventory
```

**Related Projects**:
- WMS.Inventory.API (API)
- WMS.Inventory.Application (Business logic)

---

### 10. Main Monolithic API (WMS.API) - Port 5011
**Purpose**: Fallback unified API and additional utilities

**Responsibilities**:
- Provide unified endpoints for clients
- Health checks and diagnostics
- System configuration
- Audit logging
- Fallback routing for undefined endpoints

**Key Endpoints**:
```
GET    /health                       - API health status
GET    /health/ready                 - API readiness check
GET    /status                       - System status
GET    /config                       - System configuration
GET    /audit-logs                   - System audit trail
```

**Related Projects**:
- WMS.API (Main API)
- WMS.Application (Shared business logic)
- WMS.Domain (Shared domain)
- WMS.Infrastructure (Data access)

---

### 11. Web UI (WMS.Web) - Port 5001
**Purpose**: User interface for warehouse operations

**Responsibilities**:
- Provide web interface for all 7 modules
- Form validation and submission
- User session management
- Real-time data display
- Report generation
- Integration with backend APIs

**Modules Implemented**:
1. **Product Module** - Manage products and SKU
2. **Location Module** - Manage warehouse locations
3. **Inventory Module** - Monitor stock levels
4. **Inbound Module** - Receive goods
5. **Outbound Module** - Fulfill orders
6. **Payment Module** - Process payments
7. **Delivery Module** - Track shipments

**Technologies**:
- ASP.NET Core MVC
- Bootstrap 5
- Razor Views
- JavaScript (vanilla, no framework)
- HTTP Client (ApiService)

**Related Projects**:
- WMS.Web (Web UI)

---

### 12. Domain & Infrastructure (Shared)
**Purpose**: Shared code across all services

**Responsibilities**:
- Domain models and entities
- Data access layer (EF Core)
- Repository pattern implementation
- Unit of Work pattern
- Common interfaces
- Database migrations

**Projects**:
- **WMS.Domain** - Entities, enums, interfaces, migrations
- **WMS.Application** - DTOs, common interfaces
- **WMS.Infrastructure** - EF Core context, repositories

---

## API Specifications

### Authentication Flow

#### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}

Response (200 OK):
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "user": {
      "id": "1",
      "username": "admin",
      "email": "admin@wms.local",
      "fullName": "Administrator",
      "roles": ["Admin"]
    }
  }
}
```

#### 2. All Subsequent Requests
```http
GET /api/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Accept: application/json
```

#### 3. Token Refresh
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "token": "old_token...",
  "refreshToken": "refresh_token..."
}

Response (200 OK):
{
  "token": "new_token...",
  "expiresIn": 3600
}
```

---

### Product Service API

#### List Products
```http
GET /api/products?pageNumber=1&pageSize=10&search=iPhone
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "prod_001",
        "name": "iPhone 15",
        "sku": "SKU-001",
        "category": "Electronics",
        "price": 999.99,
        "quantity": 150,
        "isActive": true,
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "totalCount": 245,
    "pageNumber": 1,
    "pageSize": 10
  },
  "message": "Products retrieved successfully"
}
```

#### Create Product
```http
POST /api/products
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "iPhone 15",
  "sku": "SKU-001",
  "category": "Electronics",
  "price": 999.99,
  "description": "Latest iPhone model"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "prod_001",
    "name": "iPhone 15",
    "sku": "SKU-001",
    "createdAt": "2024-01-28T14:30:00Z"
  },
  "message": "Product created successfully"
}
```

---

### Location Service API

#### Create Location
```http
POST /api/locations
Authorization: Bearer <token>
Content-Type: application/json

{
  "code": "ZONE-A",
  "name": "Zone A",
  "locationType": "Zone",
  "capacity": 1000,
  "parentLocationId": null,
  "description": "Main receiving zone"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "loc_001",
    "code": "ZONE-A",
    "name": "Zone A",
    "capacity": 1000,
    "currentUtilization": 0,
    "parentLocationId": null
  }
}
```

#### Get Location Tree
```http
GET /api/locations/tree
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": [
    {
      "id": "loc_001",
      "name": "Zone A",
      "code": "ZONE-A",
      "children": [
        {
          "id": "loc_002",
          "name": "Rack A1",
          "code": "RACK-A1",
          "children": [
            {
              "id": "loc_003",
              "name": "Bin A1-1",
              "code": "BIN-A1-1",
              "children": []
            }
          ]
        }
      ]
    }
  ]
}
```

---

### Inbound Service API

#### Create Inbound Order
```http
POST /api/inbound
Authorization: Bearer <token>
Content-Type: application/json

{
  "referenceNumber": "INB-2024-001",
  "supplierId": "supp_001",
  "expectedDeliveryDate": "2024-02-15",
  "items": [
    {
      "productId": "prod_001",
      "expectedQuantity": 100,
      "expectedUnit": "pcs"
    }
  ],
  "notes": "Rush delivery"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "inb_001",
    "referenceNumber": "INB-2024-001",
    "status": "Draft",
    "createdAt": "2024-01-28T14:30:00Z"
  }
}
```

#### Receive Items
```http
POST /api/inbound/{id}/receive
Authorization: Bearer <token>
Content-Type: application/json

{
  "items": [
    {
      "itemId": "inb_item_001",
      "receivedQuantity": 50,
      "locationId": "loc_001",
      "notes": "Partial shipment received"
    }
  ],
  "receivingNotes": "Quality check passed"
}

Response (200 OK):
{
  "success": true,
  "message": "Items received successfully"
}
```

#### Complete Inbound
```http
POST /api/inbound/{id}/complete
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": {
    "id": "inb_001",
    "status": "Completed",
    "totalReceived": 100,
    "updatedAt": "2024-01-28T15:45:00Z"
  }
}
```

---

### Outbound Service API

#### Create Outbound Order
```http
POST /api/outbound
Authorization: Bearer <token>
Content-Type: application/json

{
  "referenceNumber": "ORD-2024-001",
  "customerId": "cust_001",
  "customerName": "Acme Corp",
  "shippingAddress": "123 Main St, New York, NY 10001",
  "items": [
    {
      "productId": "prod_001",
      "orderedQuantity": 50
    }
  ],
  "shippingMethod": "FedEx",
  "notes": "Urgent delivery"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "outb_001",
    "referenceNumber": "ORD-2024-001",
    "status": "Draft"
  }
}
```

#### Ship Order
```http
POST /api/outbound/{id}/ship
Authorization: Bearer <token>
Content-Type: application/json

{
  "trackingNumber": "FDX-123456789",
  "shippedDate": "2024-01-28",
  "items": [
    {
      "itemId": "outb_item_001",
      "shipQuantity": 50
    }
  ],
  "shippingNotes": "Order shipped via FedEx"
}

Response (200 OK):
{
  "success": true,
  "data": {
    "id": "outb_001",
    "status": "Shipped",
    "trackingNumber": "FDX-123456789"
  }
}
```

---

### Delivery Service API

#### Create Delivery
```http
POST /api/delivery
Authorization: Bearer <token>
Content-Type: application/json

{
  "trackingNumber": "FDX-123456789",
  "outboundOrderId": "outb_001",
  "carrier": "FedEx",
  "recipientName": "John Doe",
  "recipientPhone": "+1-555-0123",
  "deliveryAddress": "123 Main St, New York, NY 10001",
  "estimatedDeliveryDate": "2024-02-02",
  "notes": "Handle with care"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "del_001",
    "trackingNumber": "FDX-123456789",
    "status": "Pending"
  }
}
```

#### Update Delivery Status
```http
PUT /api/delivery/{id}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "In Transit",
  "location": "Distribution Center, Chicago IL",
  "updateNotes": "Package in transit"
}

Response (200 OK):
{
  "success": true,
  "data": {
    "id": "del_001",
    "status": "In Transit",
    "lastUpdated": "2024-01-29T08:15:00Z"
  }
}
```

#### Public Tracking (No Auth Required)
```http
GET /api/delivery/track/FDX-123456789

Response (200 OK):
{
  "success": true,
  "data": {
    "trackingNumber": "FDX-123456789",
    "status": "In Transit",
    "carrier": "FedEx",
    "recipientName": "John Doe",
    "estimatedDeliveryDate": "2024-02-02",
    "currentLocation": "Distribution Center, Chicago IL",
    "timeline": [
      {
        "status": "Pending",
        "timestamp": "2024-01-28T14:30:00Z",
        "location": "Origin Facility"
      },
      {
        "status": "In Transit",
        "timestamp": "2024-01-29T08:15:00Z",
        "location": "Distribution Center, Chicago IL"
      }
    ]
  }
}
```

---

### Payment Service API

#### Create Payment
```http
POST /api/payment
Authorization: Bearer <token>
Content-Type: application/json

{
  "paymentNumber": "PAY-2024-001",
  "type": "Inbound",
  "referenceId": "inb_001",
  "amount": 5000.00,
  "currency": "USD",
  "method": "Bank Transfer",
  "transactionId": "TXN-123456",
  "paymentDate": "2024-01-28",
  "notes": "Payment for inbound order"
}

Response (201 Created):
{
  "success": true,
  "data": {
    "id": "pay_001",
    "paymentNumber": "PAY-2024-001",
    "status": "Pending",
    "amount": 5000.00
  }
}
```

#### Confirm Payment
```http
POST /api/payment/{id}/confirm
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": {
    "id": "pay_001",
    "status": "Completed",
    "confirmedAt": "2024-01-28T15:20:00Z"
  }
}
```

---

### Inventory Service API

#### Get Inventory Overview
```http
GET /api/inventory?pageNumber=1&pageSize=20
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": {
    "items": [
      {
        "productId": "prod_001",
        "productName": "iPhone 15",
        "sku": "SKU-001",
        "totalStock": 500,
        "locations": [
          {
            "locationId": "loc_001",
            "locationName": "Zone A",
            "quantity": 300
          },
          {
            "locationId": "loc_002",
            "locationName": "Zone B",
            "quantity": 200
          }
        ],
        "lastUpdated": "2024-01-28T14:30:00Z"
      }
    ],
    "totalCount": 125
  }
}
```

#### Get Transaction History
```http
GET /api/inventory/transactions?productId=prod_001&days=30
Authorization: Bearer <token>

Response (200 OK):
{
  "success": true,
  "data": [
    {
      "id": "txn_001",
      "type": "Inbound",
      "productId": "prod_001",
      "quantity": 100,
      "fromLocation": null,
      "toLocation": "loc_001",
      "referenceId": "inb_001",
      "timestamp": "2024-01-28T14:30:00Z"
    }
  ]
}
```

---

## Database Deployment

### Prerequisites

- Windows 10+ or SQL Server 2019+
- .NET 9 SDK installed
- SQL Server LocalDB or Express Edition
- PowerShell 5.1+ (for script execution)

### Step 1: Database Creation & Migrations

#### Option A: Using PowerShell Script (Recommended)

```powershell
# Run from project root (F:\PROJECT\STUDY\VMS)
.\setup-database.ps1
```

This script will:
1. Create WMSDB database in LocalDB
2. Apply all EF Core migrations
3. Seed initial data
4. Verify database structure

#### Option B: Manual Database Setup

```powershell
# Navigate to project root
cd F:\PROJECT\STUDY\VMS

# Update database (creates database and applies migrations)
dotnet ef database update --project WMS.Infrastructure --startup-project WMS.API

# Verify migration status
dotnet ef migrations list --project WMS.Infrastructure --startup-project WMS.API
```

#### Option C: SQL Server Management Studio

1. Connect to `(localdb)\mssqllocaldb`
2. Create new database: `WMSDB`
3. Use PowerShell script OR Entity Framework migrations

### Database Connection String

**For Local Development (LocalDB)**:
```
Server=(localdb)\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

**For Production (SQL Server)**:
```
Server=YOUR_SERVER;Database=WMSDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True
```

**Location in appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Database Schema Overview

**Core Tables** (15 tables):
- `Users` - User accounts with roles
- `Products` - Product catalog with SKU
- `Locations` - Warehouse hierarchy
- `Inventory` - Stock levels per product/location
- `InventoryTransactions` - Stock movement audit trail
- `InboundOrders` - Receiving orders
- `InboundOrderItems` - Items in inbound orders
- `OutboundOrders` - Customer orders
- `OutboundOrderItems` - Items in outbound orders
- `Deliveries` - Shipment tracking
- `Payments` - Financial transactions
- `DeliveryTimeline` - Delivery status history
- `AuditLogs` - System audit trail
- `Roles` - User roles (Admin, Manager, User, Viewer)
- `Permissions` - Role permissions

### Verify Database Setup

```powershell
# Check if database exists
Get-SqlDatabase -ServerInstance "(localdb)\mssqllocaldb" | Where-Object Name -eq "WMSDB"

# Check tables
sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"

# Expected output: 15 tables listed
```

### Seed Initial Data

The database comes with pre-populated data:
- **Admin User**: username: `admin` / password: `Admin@123`
- **Test User**: username: `user` / password: `User@123`
- **100 Sample Products**
- **5 Warehouse Zones** with hierarchical locations
- **Sample Inbound/Outbound Orders** for testing

---

## Service Startup Guide

### Startup Order

Services must be started in this order to ensure dependencies are met:

```
1. WMS.API (Port 5011)        - Backend API
2. WMS.Auth.API (Port 5002)   - Authentication
3. WMS.Products.API (Port 5003) - Products
4. WMS.Locations.API (Port 5004) - Locations
5. WMS.Inbound.API (Port 5005)   - Inbound
6. WMS.Outbound.API (Port 5006)  - Outbound
7. WMS.Payment.API (Port 5007)   - Payments
8. WMS.Delivery.API (Port 5009)  - Delivery
9. WMS.Inventory.API (Port 5010) - Inventory
10. WMS.Gateway (Port 5000)       - API Gateway
11. WMS.Web (Port 5001)           - Web UI
```

### Individual Service Startup

#### Terminal Method (PowerShell or CMD)

**Terminal 1 - WMS.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.API
dotnet run --urls "https://localhost:5011"
```

**Terminal 2 - WMS.Auth.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Auth.API
dotnet run --urls "https://localhost:5002"
```

**Terminal 3 - WMS.Products.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Products.API
dotnet run --urls "https://localhost:5003"
```

**Terminal 4 - WMS.Locations.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Locations.API
dotnet run --urls "https://localhost:5004"
```

**Terminal 5 - WMS.Inbound.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Inbound.API
dotnet run --urls "https://localhost:5005"
```

**Terminal 6 - WMS.Outbound.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Outbound.API
dotnet run --urls "https://localhost:5006"
```

**Terminal 7 - WMS.Payment.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Payment.API
dotnet run --urls "https://localhost:5007"
```

**Terminal 8 - WMS.Delivery.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Delivery.API
dotnet run --urls "https://localhost:5009"
```

**Terminal 9 - WMS.Inventory.API**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Inventory.API
dotnet run --urls "https://localhost:5010"
```

**Terminal 10 - WMS.Gateway**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Gateway
dotnet run --urls "https://localhost:5000"
```

**Terminal 11 - WMS.Web**:
```powershell
cd F:\PROJECT\STUDY\VMS\WMS.Web
dotnet run --urls "https://localhost:5001"
```

### Health Check

After all services are running, verify they're healthy:

```powershell
# Check Gateway
curl -k https://localhost:5000/health

# Check Web
curl -k https://localhost:5001/health

# Check individual APIs
curl -k https://localhost:5011/health
curl -k https://localhost:5002/health
curl -k https://localhost:5003/health
```

Expected response:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-28T16:30:45Z"
}
```

---

## Batch Scripts for Service Management

### Script 1: START_ALL_SERVICES.bat

This batch file starts all services in separate windows.

**Location**: `F:\PROJECT\STUDY\VMS\START_ALL_SERVICES.bat`

```batch
@echo off
REM WMS All Services Startup Script
REM This script starts all WMS services in separate command windows
REM Author: WMS Development Team
REM Date: January 28, 2026

echo ====================================
echo WMS - Starting All Services
echo ====================================
echo.

REM Set project root directory
set PROJECT_ROOT=F:\PROJECT\STUDY\VMS
cd /d %PROJECT_ROOT%

REM Function to start a service
REM Usage: startService ServiceName ServicePath Port

REM Start WMS.API
echo Starting WMS.API (Port 5011)...
start "WMS.API - Port 5011" cmd /k "cd %PROJECT_ROOT%\WMS.API && dotnet run --urls https://localhost:5011"
timeout /t 3 /nobreak

REM Start WMS.Auth.API
echo Starting WMS.Auth.API (Port 5002)...
start "WMS.Auth.API - Port 5002" cmd /k "cd %PROJECT_ROOT%\WMS.Auth.API && dotnet run --urls https://localhost:5002"
timeout /t 3 /nobreak

REM Start WMS.Products.API
echo Starting WMS.Products.API (Port 5003)...
start "WMS.Products.API - Port 5003" cmd /k "cd %PROJECT_ROOT%\WMS.Products.API && dotnet run --urls https://localhost:5003"
timeout /t 3 /nobreak

REM Start WMS.Locations.API
echo Starting WMS.Locations.API (Port 5004)...
start "WMS.Locations.API - Port 5004" cmd /k "cd %PROJECT_ROOT%\WMS.Locations.API && dotnet run --urls https://localhost:5004"
timeout /t 3 /nobreak

REM Start WMS.Inbound.API
echo Starting WMS.Inbound.API (Port 5005)...
start "WMS.Inbound.API - Port 5005" cmd /k "cd %PROJECT_ROOT%\WMS.Inbound.API && dotnet run --urls https://localhost:5005"
timeout /t 3 /nobreak

REM Start WMS.Outbound.API
echo Starting WMS.Outbound.API (Port 5006)...
start "WMS.Outbound.API - Port 5006" cmd /k "cd %PROJECT_ROOT%\WMS.Outbound.API && dotnet run --urls https://localhost:5006"
timeout /t 3 /nobreak

REM Start WMS.Payment.API
echo Starting WMS.Payment.API (Port 5007)...
start "WMS.Payment.API - Port 5007" cmd /k "cd %PROJECT_ROOT%\WMS.Payment.API && dotnet run --urls https://localhost:5007"
timeout /t 3 /nobreak

REM Start WMS.Delivery.API
echo Starting WMS.Delivery.API (Port 5009)...
start "WMS.Delivery.API - Port 5009" cmd /k "cd %PROJECT_ROOT%\WMS.Delivery.API && dotnet run --urls https://localhost:5009"
timeout /t 3 /nobreak

REM Start WMS.Inventory.API
echo Starting WMS.Inventory.API (Port 5010)...
start "WMS.Inventory.API - Port 5010" cmd /k "cd %PROJECT_ROOT%\WMS.Inventory.API && dotnet run --urls https://localhost:5010"
timeout /t 3 /nobreak

REM Start WMS.Gateway
echo Starting WMS.Gateway (Port 5000)...
start "WMS.Gateway - Port 5000" cmd /k "cd %PROJECT_ROOT%\WMS.Gateway && dotnet run --urls https://localhost:5000"
timeout /t 5 /nobreak

REM Start WMS.Web
echo Starting WMS.Web (Port 5001)...
start "WMS.Web - Port 5001" cmd /k "cd %PROJECT_ROOT%\WMS.Web && dotnet run --urls https://localhost:5001"
timeout /t 3 /nobreak

echo.
echo ====================================
echo All services are starting...
echo Please wait for all windows to fully load
echo ====================================
echo.
echo Access the application:
echo - Web UI:  https://localhost:5001
echo - Gateway: https://localhost:5000
echo - Swagger: https://localhost:5000/swagger
echo.
echo Press any key to close this window
pause
```

### Script 2: STOP_ALL_SERVICES.bat

This batch file stops all running WMS services.

**Location**: `F:\PROJECT\STUDY\VMS\STOP_ALL_SERVICES.bat`

```batch
@echo off
REM WMS All Services Stop Script
REM This script stops all WMS services gracefully

echo ====================================
echo WMS - Stopping All Services
echo ====================================
echo.

REM Kill services by port (requires administrator privileges)
echo Stopping services on port 5011 (WMS.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5011" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5002 (WMS.Auth.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5002" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5003 (WMS.Products.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5003" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5004 (WMS.Locations.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5004" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5005 (WMS.Inbound.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5005" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5006 (WMS.Outbound.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5006" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5007 (WMS.Payment.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5007" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5009 (WMS.Delivery.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5009" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5010 (WMS.Inventory.API)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5010" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5000 (WMS.Gateway)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5000" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo Stopping services on port 5001 (WMS.Web)...
for /f "tokens=5" %%a in ('netstat -aon ^| find ":5001" ^| find "LISTENING"') do taskkill /pid %%a /f 2>nul

echo.
echo ====================================
echo All services stopped
echo ====================================
pause
```

### Script 3: DATABASE_SETUP.bat

This batch file sets up the database.

**Location**: `F:\PROJECT\STUDY\VMS\DATABASE_SETUP.bat`

```batch
@echo off
REM WMS Database Setup Script
REM Creates WMSDB and applies all migrations

echo ====================================
echo WMS - Database Setup
echo ====================================
echo.

set PROJECT_ROOT=F:\PROJECT\STUDY\VMS
cd /d %PROJECT_ROOT%

echo Step 1: Building solution...
dotnet build WMS.sln
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Step 2: Creating/Updating database...
dotnet ef database update --project WMS.Infrastructure --startup-project WMS.API
if errorlevel 1 (
    echo Database update failed!
    pause
    exit /b 1
)

echo.
echo Step 3: Verifying database...
sqlcmd -S "(localdb)\mssqllocaldb" -d WMSDB -Q "SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"

echo.
echo ====================================
echo Database setup complete!
echo ====================================
pause
```

### Script 4: HEALTH_CHECK.bat

This batch file checks the health of all running services.

**Location**: `F:\PROJECT\STUDY\VMS\HEALTH_CHECK.bat`

```batch
@echo off
REM WMS Health Check Script

echo ====================================
echo WMS - Health Check
echo ====================================
echo.

setlocal enabledelayedexpansion

REM Array of services and ports
set services[0]=WMS.API::5011
set services[1]=WMS.Auth.API::5002
set services[2]=WMS.Products.API::5003
set services[3]=WMS.Locations.API::5004
set services[4]=WMS.Inbound.API::5005
set services[5]=WMS.Outbound.API::5006
set services[6]=WMS.Payment.API::5007
set services[7]=WMS.Delivery.API::5009
set services[8]=WMS.Inventory.API::5010
set services[9]=WMS.Gateway::5000
set services[10]=WMS.Web::5001

echo Checking service health...
echo.

for /l %%i in (0,1,10) do (
    for /f "tokens=1,2 delims=::" %%a in ("!services[%%i]!") do (
        set name=%%a
        set port=%%b
        
        echo Checking !name! on port !port!...
        curl -k https://localhost:!port!/health 2>nul
        
        if errorlevel 1 (
            echo   [OFFLINE]
        ) else (
            echo   [ONLINE]
        )
        echo.
    )
)

echo ====================================
echo Health check complete
echo ====================================
pause
```

### Usage Instructions

1. **Copy all scripts** to project root: `F:\PROJECT\STUDY\VMS\`

2. **Start all services**:
   ```batch
   START_ALL_SERVICES.bat
   ```
   This will open 11 command windows, each running one service.

3. **Access the application**:
   - Web UI: `https://localhost:5001`
   - API Gateway: `https://localhost:5000`
   - Swagger API Docs: `https://localhost:5000/swagger`

4. **Stop all services**:
   ```batch
   STOP_ALL_SERVICES.bat
   ```
   (Requires administrator privileges)

5. **Check health**:
   ```batch
   HEALTH_CHECK.bat
   ```

---

## Configuration & Environment Setup

### JWT Configuration

**Location**: `appsettings.json` in each API project

```json
{
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
    "Issuer": "WMS.API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**For Production**: 
- Generate a new secret key (min 32 characters)
- Update all API projects with same key
- Use environment variables instead of hardcoding

### CORS Configuration

**Location**: Gateway and Web UI `appsettings.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:5001",
      "https://localhost:5000",
      "https://yourdomain.com"
    ]
  }
}
```

### API Gateway Configuration

**Location**: `WMS.Gateway\appsettings.json`

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-products": {
        "ClusterId": "productsCluster",
        "Match": { "Path": "/api/products{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/products{**catch-all}" }]
      }
      // ... more routes
    },
    "Clusters": {
      "productsCluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://localhost:5003/"
          }
        }
      }
      // ... more clusters
    }
  }
}
```

### Environment Variables (Optional)

Create `.env` file in project root:

```
# Database
DB_CONNECTION_STRING=Server=(localdb)\mssqllocaldb;Database=WMSDB;Trusted_Connection=True

# JWT
JWT_SECRET_KEY=YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters
JWT_ISSUER=WMS.API
JWT_AUDIENCE=WMS.Client

# CORS
ALLOWED_ORIGINS=https://localhost:5001,https://localhost:5000

# Logging
LOG_LEVEL=Information
```

---

## Troubleshooting

### Port Already in Use

**Error**: `System.IO.IOException: Failed to bind to address https://127.0.0.1:5001`

**Solution**:
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (replace PID)
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### Database Connection Failed

**Error**: `SqlException: Cannot open database "WMSDB"`

**Solution**:
```powershell
# Verify LocalDB is installed
Get-Command sqllocaldb

# Create database
sqllocaldb create WMSDB

# Apply migrations
dotnet ef database update --project WMS.Infrastructure
```

### JWT Token Errors

**Error**: `"The token is invalid"`

**Solution**:
- Ensure JWT secret key is same across all APIs
- Check token expiration time
- Verify token format in Authorization header: `Bearer <token>`
- Check system clock is synchronized

### Service Dependencies Not Found

**Error**: `Unable to load one or more of the requested types`

**Solution**:
1. Ensure all projects are built: `dotnet build`
2. Check NuGet packages: `dotnet restore`
3. Verify project references are correct
4. Restart Visual Studio / rebuild solution

### CORS Errors

**Error**: `Access to XMLHttpRequest has been blocked by CORS policy`

**Solution**:
1. Check Gateway CORS configuration
2. Verify client origin is in AllowedOrigins
3. Restart services after config change
4. Check browser console for exact error

### Service Takes Too Long to Start

**Solution**:
- Cold start (first run) is slower due to compilation
- Subsequent runs are faster (cached compiled output)
- Startup typically takes 5-10 seconds per service
- Use `dotnet run --no-build` to skip compilation

### Database Migrations Failed

**Error**: `The migrations assembly in the data project does not match the startup project`

**Solution**:
```powershell
# Specify correct projects
dotnet ef database update `
  --project WMS.Infrastructure `
  --startup-project WMS.API `
  --configuration Debug `
  --verbose
```

### Can't Login to Web Application

**Default Credentials**:
- Username: `admin`
- Password: `Admin@123`

If login fails:
1. Verify database is populated with seed data
2. Check Auth API is running on port 5002
3. Check JWT token in session storage (F12 → Application tab)
4. Clear browser cookies and try again

---

## Performance Considerations

### Optimization Tips

1. **Enable Response Caching**:
   ```csharp
   services.AddResponseCaching();
   app.UseResponseCaching();
   ```

2. **Database Query Optimization**:
   - Use `.AsNoTracking()` for read-only queries
   - Implement query pagination
   - Add appropriate database indexes

3. **API Response Compression**:
   ```csharp
   services.AddResponseCompression();
   app.UseResponseCompression();
   ```

4. **Async/Await**:
   - Use async methods throughout
   - Avoid `.Result` and `.Wait()`

5. **Connection Pooling**:
   - Configured by default in EF Core
   - Adjust `Max Pool Size` if needed

### Scaling Strategies

1. **Horizontal Scaling**:
   - Load balance API Gateway
   - Multiple instances of each microservice
   - Shared database (SQL Server Replication)

2. **Caching**:
   - Implement Redis for distributed caching
   - Cache product and location data
   - Cache JWT tokens

3. **Database**:
   - Regular backups
   - Index optimization
   - Query plan analysis
   - Consider read replicas for reporting

---

## Security Best Practices

### Production Checklist

- [ ] Use HTTPS everywhere (SSL certificates)
- [ ] Rotate JWT secret key regularly
- [ ] Implement rate limiting on API Gateway
- [ ] Enable CORS only for known origins
- [ ] Use environment variables for sensitive data
- [ ] Implement API key authentication
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Database encryption at rest
- [ ] Implement request validation
- [ ] Use strong password policies
- [ ] Enable CORS headers security
- [ ] Implement API versioning
- [ ] Add request timeout limits

---

## Monitoring & Logging

### Health Endpoints

All services expose health check endpoints:

```
GET /health           - Service is running
GET /health/ready     - Service is ready to serve
GET /health/live      - Service is alive
```

### Logging

Logs are written to:
- **Console** (development)
- **File** (optional, configure in appsettings.json)
- **Application Insights** (optional, Azure)

### Metrics to Monitor

- Average response time per API
- Request rate (requests/sec)
- Error rate (5xx responses)
- Database query performance
- Cache hit/miss ratio
- JWT token expiration issues
- Port availability

---

## Deployment

### Development Deployment

Already configured in this guide (localhost).

### Staging Deployment

Update connection strings and ports:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-sql-server;Database=WMSDB_Staging;..."
  }
}
```

### Production Deployment

Options:
1. **Windows Server + IIS**
2. **Azure App Services**
3. **Docker + Kubernetes**
4. **On-premises VM**

---

## Summary

This WMS system is production-ready with:
- ✅ 10 microservices + 1 gateway
- ✅ Comprehensive API specifications
- ✅ Complete database setup guide
- ✅ Automated startup scripts
- ✅ Health check and monitoring
- ✅ Security best practices
- ✅ Troubleshooting guide
- ✅ Scaling strategies

**Total Components**: 11 services
**Total Ports**: 11 unique ports (5000-5011)
**Database**: Single shared WMSDB
**Architecture**: Clean + Microservices + API Gateway
**Status**: Ready for deployment

---

**For questions or issues, consult the troubleshooting section or review service-specific appsettings.json files.**

**Last Updated**: January 28, 2026
**Version**: 1.0
**Status**: Production Ready ✅
