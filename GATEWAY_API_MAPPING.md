# ?? WMS API Gateway - Complete Endpoint Mapping

## ?? Gateway Configuration Overview

**Gateway URL:** `https://localhost:7000`

All microservices are accessible through the API Gateway with unified routing and authentication.

---

## ?? Authentication API (Port: 7081)

### Base Path: `/auth`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| POST | `/auth/login` | User login | `/api/auth/login` |
| POST | `/auth/register` | User registration | `/api/auth/register` |
| POST | `/auth/refresh` | Refresh access token | `/api/auth/refresh` |
| GET | `/auth/me` | Get current user profile | `/api/auth/me` |
| GET | `/auth/validate` | Validate token | `/api/auth/validate` |

**Example:**
```bash
# Login through Gateway
POST https://localhost:7000/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}
```

---

## ?? Products API (Port: 62527)

### Base Path: `/products`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/products` | Get all products (paginated) | `/api/products` |
| GET | `/products/{id}` | Get product by ID | `/api/products/{id}` |
| GET | `/products/sku/{sku}` | Get product by SKU | `/api/products/sku/{sku}` |
| POST | `/products` | Create new product | `/api/products` |
| PUT | `/products/{id}` | Update product | `/api/products/{id}` |
| PATCH | `/products/{id}/activate` | Activate product | `/api/products/{id}/activate` |
| PATCH | `/products/{id}/deactivate` | Deactivate product | `/api/products/{id}/deactivate` |

**Example:**
```bash
# Get all products through Gateway
GET https://localhost:7000/products?pageNumber=1&pageSize=20

# Create product
POST https://localhost:7000/products
{
  "sku": "PROD001",
  "name": "Sample Product",
  "description": "Product description",
  "uom": "PCS"
}
```

---

## ?? Locations API (Port: 62522)

### Base Path: `/locations`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/locations` | Get all locations (paginated) | `/api/locations` |
| GET | `/locations/{id}` | Get location by ID | `/api/locations/{id}` |
| GET | `/locations/code/{code}` | Get location by code | `/api/locations/code/{code}` |
| POST | `/locations` | Create new location | `/api/locations` |
| PUT | `/locations/{id}` | Update location | `/api/locations/{id}` |
| PATCH | `/locations/{id}/activate` | Activate location | `/api/locations/{id}/activate` |
| PATCH | `/locations/{id}/deactivate` | Deactivate location | `/api/locations/{id}/deactivate` |

**Example:**
```bash
# Get location by code
GET https://localhost:7000/locations/code/A-01-01

# Create location
POST https://localhost:7000/locations
{
  "code": "A-01-01",
  "name": "Zone A - Aisle 1 - Rack 1",
  "type": "Rack",
  "capacity": 1000
}
```

---

## ?? Inventory API (Port: 62531)

### Base Path: `/inventory`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/inventory` | Get all inventory (paginated) | `/api/inventory` |
| GET | `/inventory/{id}` | Get inventory by ID | `/api/inventory/{id}` |
| GET | `/inventory/product/{productId}` | Get inventory by product | `/api/inventory/product/{productId}` |
| GET | `/inventory/location/{locationId}` | Get inventory by location | `/api/inventory/location/{locationId}` |
| GET | `/inventory/transactions` | Get inventory transactions | `/api/inventory/transactions` |
| POST | `/inventory/adjust` | Adjust inventory | `/api/inventory/adjust` |
| POST | `/inventory/transfer` | Transfer inventory | `/api/inventory/transfer` |

**Example:**
```bash
# Get inventory for a product
GET https://localhost:7000/inventory/product/12345678-1234-1234-1234-123456789012

# Get inventory transactions
GET https://localhost:7000/inventory/transactions?pageNumber=1&pageSize=20&productId=12345678-1234-1234-1234-123456789012
```

---

## ?? Inbound API (Port: 62520)

### Base Path: `/inbound`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/inbound` | Get all inbounds (paginated) | `/api/inbound` |
| GET | `/inbound/{id}` | Get inbound by ID | `/api/inbound/{id}` |
| POST | `/inbound` | Create new inbound | `/api/inbound` |
| POST | `/inbound/receive` | Receive inbound items | `/api/inbound/receive` |
| POST | `/inbound/{id}/cancel` | Cancel inbound | `/api/inbound/{id}/cancel` |

**Example:**
```bash
# Create inbound
POST https://localhost:7000/inbound
{
  "supplierName": "Supplier ABC",
  "expectedDate": "2024-01-30",
  "items": [
    {
      "productId": "12345678-1234-1234-1234-123456789012",
      "locationId": "87654321-4321-4321-4321-210987654321",
      "orderedQuantity": 100
    }
  ]
}

# Receive inbound
POST https://localhost:7000/inbound/receive
{
  "inboundId": "12345678-1234-1234-1234-123456789012",
  "items": [
    {
      "inboundItemId": "87654321-4321-4321-4321-210987654321",
      "receivedQuantity": 100
    }
  ]
}
```

---

## ?? Outbound API (Port: 62519)

### Base Path: `/outbound`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/outbound` | Get all outbounds (paginated) | `/api/outbound` |
| GET | `/outbound/{id}` | Get outbound by ID | `/api/outbound/{id}` |
| POST | `/outbound` | Create new outbound | `/api/outbound` |
| POST | `/outbound/pick` | Pick outbound items | `/api/outbound/pick` |
| POST | `/outbound/ship` | Ship outbound | `/api/outbound/ship` |
| POST | `/outbound/{id}/cancel` | Cancel outbound | `/api/outbound/{id}/cancel` |

**Example:**
```bash
# Create outbound
POST https://localhost:7000/outbound
{
  "customerName": "Customer XYZ",
  "shippingAddress": "123 Main St, City",
  "items": [
    {
      "productId": "12345678-1234-1234-1234-123456789012",
      "locationId": "87654321-4321-4321-4321-210987654321",
      "orderedQuantity": 50
    }
  ]
}

# Pick outbound
POST https://localhost:7000/outbound/pick
{
  "outboundId": "12345678-1234-1234-1234-123456789012",
  "items": [
    {
      "outboundItemId": "87654321-4321-4321-4321-210987654321",
      "pickedQuantity": 50
    }
  ]
}

# Ship outbound
POST https://localhost:7000/outbound/ship
{
  "outboundId": "12345678-1234-1234-1234-123456789012"
}
```

---

## ?? Payment API (Port: 62521)

### Base Path: `/payment`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/payment` | Get all payments (paginated) | `/api/payment` |
| GET | `/payment/{id}` | Get payment by ID | `/api/payment/{id}` |
| POST | `/payment` | Create new payment | `/api/payment` |
| POST | `/payment/confirm` | Confirm payment | `/api/payment/confirm` |
| POST | `/payment/{id}/cancel` | Cancel payment | `/api/payment/{id}/cancel` |

**Example:**
```bash
# Create payment
POST https://localhost:7000/payment
{
  "outboundId": "12345678-1234-1234-1234-123456789012",
  "paymentType": "Prepaid",
  "amount": 1000.00,
  "currency": "USD",
  "paymentMethod": "Credit Card"
}

# Confirm payment
POST https://localhost:7000/payment/confirm
{
  "paymentId": "12345678-1234-1234-1234-123456789012",
  "externalPaymentId": "PAY123456",
  "transactionReference": "TXN789012"
}
```

---

## ?? Delivery API (Port: 62529)

### Base Path: `/delivery`

| Method | Endpoint | Description | Target API |
|--------|----------|-------------|------------|
| GET | `/delivery` | Get all deliveries (paginated) | `/api/delivery` |
| GET | `/delivery/{id}` | Get delivery by ID | `/api/delivery/{id}` |
| GET | `/delivery/tracking/{trackingNumber}` | Track delivery (public) | `/api/delivery/tracking/{trackingNumber}` |
| POST | `/delivery` | Create new delivery | `/api/delivery` |
| PUT | `/delivery/status` | Update delivery status | `/api/delivery/status` |
| POST | `/delivery/complete` | Complete delivery | `/api/delivery/complete` |
| POST | `/delivery/fail` | Mark delivery as failed | `/api/delivery/fail` |
| POST | `/delivery/event` | Add delivery event | `/api/delivery/event` |

**Example:**
```bash
# Track delivery (public - no auth required)
GET https://localhost:7000/delivery/tracking/DEL-20240124-0001

# Create delivery
POST https://localhost:7000/delivery
{
  "outboundId": "12345678-1234-1234-1234-123456789012",
  "shippingAddress": "123 Main St, City",
  "carrier": "FedEx",
  "trackingNumber": "DEL-20240124-0001"
}

# Update delivery status
PUT https://localhost:7000/delivery/status
{
  "deliveryId": "12345678-1234-1234-1234-123456789012",
  "status": "InTransit",
  "eventLocation": "Distribution Center",
  "notes": "Package in transit"
}

# Complete delivery
POST https://localhost:7000/delivery/complete
{
  "deliveryId": "12345678-1234-1234-1234-123456789012",
  "notes": "Delivered successfully"
}
```

---

## ?? Authentication Flow

### 1. Login
```bash
POST https://localhost:7000/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}

# Response:
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "username": "admin",
    "role": "Admin"
  }
}
```

### 2. Use Access Token
```bash
GET https://localhost:7000/products
Authorization: Bearer eyJhbGc...
```

### 3. Refresh Token
```bash
POST https://localhost:7000/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh_token_here"
}
```

---

## ?? Common Query Parameters

### Pagination
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)

### Filtering
- `status` - Filter by status (where applicable)
- `searchTerm` - Search term (where applicable)

**Example:**
```bash
GET https://localhost:7000/products?pageNumber=1&pageSize=20&searchTerm=laptop
GET https://localhost:7000/inbound?status=Pending&pageNumber=1
```

---

## ?? Quick Start

### 1. Start the Gateway
```powershell
cd WMS.Gateway
dotnet run
```

### 2. Start All Microservices
```powershell
.\run-all-services.ps1
```

### 3. Test Gateway
```bash
# Health check (if implemented)
GET https://localhost:7000/health

# Login
POST https://localhost:7000/auth/login
```

---

## ?? Service Ports Summary

| Service | HTTPS Port | Gateway Path |
|---------|------------|--------------|
| Auth API | 7081 | `/auth` |
| Products API | 62527 | `/products` |
| Locations API | 62522 | `/locations` |
| Inventory API | 62531 | `/inventory` |
| Inbound API | 62520 | `/inbound` |
| Outbound API | 62519 | `/outbound` |
| Payment API | 62521 | `/payment` |
| Delivery API | 62529 | `/delivery` |

---

## ?? Gateway Features

? **Unified Entry Point** - Single URL for all microservices  
? **Route Transformation** - Clean URLs mapped to internal APIs  
? **Load Balancing** - Built-in with YARP  
? **SSL/TLS Support** - HTTPS enabled  
? **CORS Handling** - Centralized CORS configuration  
? **Logging** - Comprehensive request/response logging  

---

## ?? Configuration

The gateway configuration is in `WMS.Gateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": { ... },
    "Clusters": { ... }
  }
}
```

### Adding New Routes

1. Add route configuration:
```json
"new-route": {
  "ClusterId": "service-cluster",
  "Match": {
    "Path": "/newpath/{**catch-all}"
  },
  "Transforms": [
    {
      "PathPattern": "/api/{**catch-all}"
    }
  ]
}
```

2. Add or update cluster:
```json
"service-cluster": {
  "Destinations": {
    "service-api": {
      "Address": "https://localhost:PORT"
    }
  }
}
```

---

## ?? Notes

- All routes require authentication except `/auth/login`, `/auth/register`, and `/delivery/tracking/{trackingNumber}`
- Gateway runs on port 7000 (HTTPS)
- All microservices use HTTPS
- JWT tokens are passed through to microservices
- Query parameters and request bodies are forwarded automatically

---

**Last Updated:** 2024-01-24  
**Gateway Version:** 1.0  
**Status:** ? Production Ready
