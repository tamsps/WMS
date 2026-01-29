# WMS - Complete API Specification

**Version**: 2.0  
**Date**: January 28, 2026  
**Base URL**: `https://localhost:5000/api` (via API Gateway)  
**Protocol**: REST / JSON  
**Authentication**: JWT Bearer Token

---

## Table of Contents

1. [Authentication API](#authentication-api)
2. [Product API](#product-api)
3. [Location API](#location-api)
4. [Inventory API](#inventory-api)
5. [Inbound API](#inbound-api)
6. [Outbound API](#outbound-api)
7. [Payment API](#payment-api)
8. [Delivery API](#delivery-api)
9. [Common Response Formats](#common-response-formats)
10. [Error Handling](#error-handling)
11. [Rate Limiting](#rate-limiting)

---

## API Overview

### Service Ports & Base URLs

| Service | Port | Base URL | Description |
|---------|------|----------|-------------|
| API Gateway | 5000 | `https://localhost:5000/api` | Central entry point |
| Auth API | 5002 | `https://localhost:5002/api` | Authentication & authorization |
| Products API | 5003 | `https://localhost:5003/api` | Product management |
| Locations API | 5004 | `https://localhost:5004/api` | Warehouse locations |
| Inbound API | 5005 | `https://localhost:5005/api` | Goods receiving |
| Outbound API | 5006 | `https://localhost:5006/api` | Order fulfillment |
| Payment API | 5007 | `https://localhost:5007/api` | Payment processing |
| Delivery API | 5009 | `https://localhost:5009/api` | Delivery management |
| Inventory API | 5010 | `https://localhost:5010/api` | Stock management |

---

## Authentication API

**Service Port**: 5002  
**Base URL**: `https://localhost:5000/api/auth` (via Gateway)

### 1. Login

Authenticate user and obtain JWT token.

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkFkbWluIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "admin",
      "email": "admin@wms.local",
      "fullName": "Administrator",
      "roles": ["Admin"]
    }
  }
}
```

**Error (401 Unauthorized)**:
```json
{
  "success": false,
  "message": "Invalid credentials",
  "errors": ["Username or password is incorrect"]
}
```

---

### 2. Register

Create a new user account.

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "newuser@wms.local",
  "password": "SecurePass@123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "username": "newuser",
    "email": "newuser@wms.local"
  }
}
```

**Error (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Registration failed",
  "errors": ["Username already exists", "Password does not meet requirements"]
}
```

---

### 3. Refresh Token

Get a new JWT token using refresh token.

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600
  }
}
```

---

### 4. Get Current User

Retrieve authenticated user's profile.

```http
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "admin",
    "email": "admin@wms.local",
    "fullName": "Administrator",
    "roles": ["Admin"],
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

### 5. Logout

Invalidate current session.

```http
POST /api/auth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

## Product API

**Service Port**: 5003  
**Base URL**: `https://localhost:5000/api/products`

### 1. List Products

Retrieve paginated list of products.

```http
GET /api/products?page=1&pageSize=20&search=laptop&category=Electronics
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440010",
        "name": "Dell Laptop XPS 13",
        "sku": "DELL-XPS13-001",
        "description": "Premium ultrabook laptop",
        "price": 999.99,
        "category": "Electronics",
        "isActive": true,
        "createdAt": "2026-01-15T10:30:00Z"
      }
    ],
    "totalCount": 45,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

**Query Parameters**:
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)
- `search` (optional): Search by name or SKU
- `category` (optional): Filter by category
- `isActive` (optional): Filter by status

---

### 2. Get Product by ID

Retrieve specific product details.

```http
GET /api/products/{id}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "name": "Dell Laptop XPS 13",
    "sku": "DELL-XPS13-001",
    "description": "Premium ultrabook laptop",
    "price": 999.99,
    "category": "Electronics",
    "isActive": true,
    "createdAt": "2026-01-15T10:30:00Z",
    "updatedAt": "2026-01-20T14:15:00Z"
  }
}
```

---

### 3. Create Product

Add new product to catalog.

```http
POST /api/products
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Samsung Monitor 27\"",
  "sku": "SAMSUNG-MON27-001",
  "description": "4K UHD Monitor",
  "price": 349.99,
  "category": "Electronics"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440011",
    "name": "Samsung Monitor 27\"",
    "sku": "SAMSUNG-MON27-001",
    "price": 349.99
  }
}
```

---

### 4. Update Product

Modify existing product.

```http
PUT /api/products/{id}
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Samsung Monitor 27\" (Updated)",
  "price": 329.99,
  "category": "Electronics"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Product updated successfully"
}
```

---

### 5. Activate Product

Enable product for sale.

```http
PATCH /api/products/{id}/activate
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Product activated"
}
```

---

### 6. Deactivate Product

Disable product from sale.

```http
PATCH /api/products/{id}/deactivate
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Product deactivated"
}
```

---

## Location API

**Service Port**: 5004  
**Base URL**: `https://localhost:5000/api/locations`

### 1. List Locations

Retrieve all warehouse locations.

```http
GET /api/locations?page=1&pageSize=50&type=Zone
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440020",
        "code": "ZONE-A",
        "name": "Zone A - Electronics",
        "type": "Zone",
        "capacity": 1000,
        "isActive": true
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 50
  }
}
```

---

### 2. Get Location by ID

```http
GET /api/locations/{id}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440020",
    "code": "ZONE-A",
    "name": "Zone A - Electronics",
    "type": "Zone",
    "capacity": 1000,
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

### 3. Create Location

```http
POST /api/locations
Content-Type: application/json
Authorization: Bearer {token}

{
  "code": "AISLE-A-01",
  "name": "Aisle A-01",
  "type": "Aisle",
  "capacity": 500
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440021",
    "code": "AISLE-A-01"
  }
}
```

---

### 4. Update Location

```http
PUT /api/locations/{id}
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Aisle A-01 (Updated)",
  "capacity": 600
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Location updated"
}
```

---

### 5. Activate/Deactivate Location

```http
PATCH /api/locations/{id}/activate
PATCH /api/locations/{id}/deactivate
Authorization: Bearer {token}
```

---

## Inventory API

**Service Port**: 5010  
**Base URL**: `https://localhost:5000/api/inventory`

### 1. Get All Inventory Records

```http
GET /api/inventory?page=1&pageSize=20
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440030",
        "productId": "550e8400-e29b-41d4-a716-446655440010",
        "productName": "Dell Laptop XPS 13",
        "locationId": "550e8400-e29b-41d4-a716-446655440020",
        "locationCode": "ZONE-A",
        "quantity": 50,
        "reservedQuantity": 5,
        "availableQuantity": 45,
        "lastCountDate": "2026-01-20T10:00:00Z"
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### 2. Get Product Inventory

```http
GET /api/inventory/product/{productId}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440030",
      "locationId": "550e8400-e29b-41d4-a716-446655440020",
      "locationCode": "ZONE-A",
      "quantity": 50,
      "availableQuantity": 45
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440031",
      "locationId": "550e8400-e29b-41d4-a716-446655440021",
      "locationCode": "ZONE-B",
      "quantity": 30,
      "availableQuantity": 30
    }
  ]
}
```

---

### 3. Get Inventory Levels

```http
GET /api/inventory/levels?lowStock=true
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440010",
      "productName": "Dell Laptop XPS 13",
      "totalQuantity": 80,
      "totalAvailable": 75,
      "minThreshold": 20,
      "isLowStock": false
    }
  ]
}
```

---

### 4. Get Inventory Transactions

```http
GET /api/inventory/transactions?page=1&pageSize=50&type=Inbound
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440040",
        "inventoryRecordId": "550e8400-e29b-41d4-a716-446655440030",
        "transactionType": "Inbound",
        "quantity": 50,
        "reference": "INBOUND-001",
        "notes": "Received from supplier",
        "createdAt": "2026-01-20T10:30:00Z",
        "createdBy": "admin"
      }
    ],
    "totalCount": 500,
    "page": 1,
    "pageSize": 50
  }
}
```

---

### 5. Adjust Inventory

```http
POST /api/inventory/adjust
Content-Type: application/json
Authorization: Bearer {token}

{
  "inventoryRecordId": "550e8400-e29b-41d4-a716-446655440030",
  "quantity": 10,
  "transactionType": "Adjustment",
  "reason": "Physical count variance"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Inventory adjusted",
  "data": {
    "newQuantity": 60,
    "transactionId": "550e8400-e29b-41d4-a716-446655440041"
  }
}
```

---

## Inbound API

**Service Port**: 5005  
**Base URL**: `https://localhost:5000/api/inbound`

### 1. Create Inbound Order

```http
POST /api/inbound/orders
Content-Type: application/json
Authorization: Bearer {token}

{
  "supplierName": "Tech Supplies Inc",
  "expectedDate": "2026-01-25T00:00:00Z",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440010",
      "quantityExpected": 50
    }
  ]
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Inbound order created",
  "data": {
    "orderId": "550e8400-e29b-41d4-a716-446655440050",
    "orderNumber": "INBOUND-001",
    "status": "Pending"
  }
}
```

---

### 2. Get Inbound Order

```http
GET /api/inbound/orders/{orderId}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440050",
    "orderNumber": "INBOUND-001",
    "status": "Receiving",
    "supplierName": "Tech Supplies Inc",
    "expectedDate": "2026-01-25T00:00:00Z",
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440051",
        "productId": "550e8400-e29b-41d4-a716-446655440010",
        "productName": "Dell Laptop XPS 13",
        "quantityExpected": 50,
        "quantityReceived": 45,
        "status": "Receiving"
      }
    ],
    "createdAt": "2026-01-20T10:00:00Z"
  }
}
```

---

### 3. Receive Item

```http
POST /api/inbound/orders/{orderId}/receive-item
Content-Type: application/json
Authorization: Bearer {token}

{
  "itemId": "550e8400-e29b-41d4-a716-446655440051",
  "quantityReceived": 10,
  "conditionStatus": "Good"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Item received",
  "data": {
    "itemId": "550e8400-e29b-41d4-a716-446655440051",
    "totalReceived": 45
  }
}
```

---

### 4. Putaway Item

```http
POST /api/inbound/orders/{orderId}/putaway
Content-Type: application/json
Authorization: Bearer {token}

{
  "itemId": "550e8400-e29b-41d4-a716-446655440051",
  "locationId": "550e8400-e29b-41d4-a716-446655440020",
  "quantity": 45
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Item putaway completed",
  "data": {
    "locationCode": "ZONE-A",
    "inventoryRecordId": "550e8400-e29b-41d4-a716-446655440030"
  }
}
```

---

## Outbound API

**Service Port**: 5006  
**Base URL**: `https://localhost:5000/api/outbound`

### 1. Create Outbound Order

```http
POST /api/outbound/orders
Content-Type: application/json
Authorization: Bearer {token}

{
  "customerName": "ABC Retail Inc",
  "shippingAddress": "123 Main St, City, State 12345",
  "dueDate": "2026-01-28T00:00:00Z",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440010",
      "quantityRequired": 10
    }
  ]
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Outbound order created",
  "data": {
    "orderId": "550e8400-e29b-41d4-a716-446655440060",
    "orderNumber": "OUTBOUND-001",
    "status": "Pending"
  }
}
```

---

### 2. Get Outbound Order

```http
GET /api/outbound/orders/{orderId}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440060",
    "orderNumber": "OUTBOUND-001",
    "status": "Picking",
    "customerName": "ABC Retail Inc",
    "shippingAddress": "123 Main St, City, State 12345",
    "dueDate": "2026-01-28T00:00:00Z",
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440061",
        "productId": "550e8400-e29b-41d4-a716-446655440010",
        "productName": "Dell Laptop XPS 13",
        "quantityRequired": 10,
        "quantityPicked": 8,
        "status": "Picking"
      }
    ]
  }
}
```

---

### 3. Pick Item

```http
POST /api/outbound/orders/{orderId}/pick
Content-Type: application/json
Authorization: Bearer {token}

{
  "itemId": "550e8400-e29b-41d4-a716-446655440061",
  "locationId": "550e8400-e29b-41d4-a716-446655440020",
  "quantityPicked": 10,
  "pickDate": "2026-01-20T14:00:00Z"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Item picked",
  "data": {
    "itemId": "550e8400-e29b-41d4-a716-446655440061",
    "quantityPicked": 10
  }
}
```

---

### 4. Pack Shipment

```http
POST /api/outbound/orders/{orderId}/pack
Content-Type: application/json
Authorization: Bearer {token}

{
  "cartonNumber": "CARTON-001",
  "weight": 15.5,
  "items": [
    {
      "itemId": "550e8400-e29b-41d4-a716-446655440061",
      "quantityPacked": 10
    }
  ]
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Items packed",
  "data": {
    "cartonNumber": "CARTON-001"
  }
}
```

---

### 5. Ship Order

```http
POST /api/outbound/orders/{orderId}/ship
Content-Type: application/json
Authorization: Bearer {token}

{
  "carrier": "FedEx",
  "trackingNumber": "794698374629837",
  "shippingDate": "2026-01-20T15:00:00Z"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Order shipped",
  "data": {
    "orderId": "550e8400-e29b-41d4-a716-446655440060",
    "status": "Shipped",
    "trackingNumber": "794698374629837"
  }
}
```

---

## Payment API

**Service Port**: 5007  
**Base URL**: `https://localhost:5000/api/payment`

### 1. Process Payment

```http
POST /api/payment/process
Content-Type: application/json
Authorization: Bearer {token}

{
  "orderId": "550e8400-e29b-41d4-a716-446655440060",
  "amount": 9999.90,
  "paymentMethod": "CreditCard",
  "cardDetails": {
    "cardholderName": "John Doe",
    "cardNumber": "****1234",
    "expiryDate": "12/25",
    "cvv": "***"
  }
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Payment processed successfully",
  "data": {
    "transactionId": "550e8400-e29b-41d4-a716-446655440070",
    "transactionNumber": "TXN-2026-001",
    "status": "Completed",
    "amount": 9999.90
  }
}
```

---

### 2. Get Payment Transaction

```http
GET /api/payment/transactions/{transactionId}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440070",
    "transactionNumber": "TXN-2026-001",
    "orderId": "550e8400-e29b-41d4-a716-446655440060",
    "amount": 9999.90,
    "status": "Completed",
    "paymentMethod": "CreditCard",
    "processedAt": "2026-01-20T15:30:00Z"
  }
}
```

---

### 3. Get Payment History

```http
GET /api/payment/history?page=1&pageSize=20&status=Completed
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "transactionId": "550e8400-e29b-41d4-a716-446655440070",
        "transactionNumber": "TXN-2026-001",
        "amount": 9999.90,
        "status": "Completed",
        "processedAt": "2026-01-20T15:30:00Z"
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### 4. Reconcile Payments

```http
POST /api/payment/reconcile
Content-Type: application/json
Authorization: Bearer {token}

{
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-01-31T23:59:59Z"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "totalTransactions": 150,
    "totalAmount": 149999.50,
    "reconciled": 145,
    "pending": 5,
    "discrepancies": []
  }
}
```

---

## Delivery API

**Service Port**: 5009  
**Base URL**: `https://localhost:5000/api/delivery`

### 1. Create Delivery Route

```http
POST /api/delivery/routes
Content-Type: application/json
Authorization: Bearer {token}

{
  "driverName": "John Smith",
  "vehicleId": "VEH-001",
  "startDate": "2026-01-21T06:00:00Z",
  "stops": [
    {
      "outboundOrderId": "550e8400-e29b-41d4-a716-446655440060",
      "sequence": 1,
      "address": "123 Main St, City, State 12345"
    }
  ]
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Delivery route created",
  "data": {
    "routeId": "550e8400-e29b-41d4-a716-446655440080",
    "routeNumber": "ROUTE-2026-001",
    "status": "Pending"
  }
}
```

---

### 2. List Delivery Routes

```http
GET /api/delivery/routes?page=1&status=InProgress
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440080",
        "routeNumber": "ROUTE-2026-001",
        "status": "InProgress",
        "driverName": "John Smith",
        "vehicleId": "VEH-001",
        "startDate": "2026-01-21T06:00:00Z",
        "stopCount": 5
      }
    ],
    "totalCount": 10,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### 3. Track Delivery

```http
GET /api/delivery/routes/{routeId}/tracking
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "routeId": "550e8400-e29b-41d4-a716-446655440080",
    "routeNumber": "ROUTE-2026-001",
    "status": "InProgress",
    "completedStops": 2,
    "totalStops": 5,
    "currentLocation": {
      "latitude": 40.7128,
      "longitude": -74.0060,
      "address": "789 Oak Ave, City, State 12345"
    },
    "stops": [
      {
        "sequence": 1,
        "orderId": "550e8400-e29b-41d4-a716-446655440060",
        "status": "Delivered",
        "deliveredAt": "2026-01-21T08:15:00Z"
      }
    ]
  }
}
```

---

### 4. Complete Delivery

```http
POST /api/delivery/routes/{routeId}/complete
Content-Type: application/json
Authorization: Bearer {token}

{
  "completionDate": "2026-01-21T18:00:00Z",
  "totalDistance": 125.5,
  "totalTime": 12.5
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Delivery route completed",
  "data": {
    "routeId": "550e8400-e29b-41d4-a716-446655440080",
    "status": "Completed"
  }
}
```

---

## Common Response Formats

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "...",
    "name": "..."
  },
  "timestamp": "2026-01-20T15:30:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Operation failed",
  "errors": [
    "Field validation error",
    "Business rule violation"
  ],
  "timestamp": "2026-01-20T15:30:00Z"
}
```

### Paginated Response
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate SKU, etc |
| 422 | Unprocessable Entity | Validation failure |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Server Error | Unexpected error |

### Error Response Example
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "SKU 'SAMSUNG-MON27-001' already exists",
    "Price must be greater than 0"
  ],
  "code": "VALIDATION_ERROR",
  "timestamp": "2026-01-20T15:30:00Z"
}
```

---

## Rate Limiting

### Limits
- **Default**: 1000 requests per hour per IP
- **Authenticated**: 5000 requests per hour per user
- **Admin**: Unlimited

### Headers
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1674240600
```

### 429 Response (Too Many Requests)
```json
{
  "success": false,
  "message": "Rate limit exceeded",
  "retryAfter": 60
}
```

---

## Authentication Headers

All endpoints (except `/api/auth/login` and `/api/auth/register`) require:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIn0...
```

---

## Postman Collection

Import the Postman collection for testing:
- File: `WMS_Gateway_Postman_Collection.json`
- Location: Root of repository
- Contains all endpoints with example requests

---

## Summary

The WMS API provides complete RESTful access to all warehouse operations:

- ✅ **8 Microservices** with dedicated endpoints
- ✅ **JWT Authentication** for security
- ✅ **Pagination** for large datasets
- ✅ **Comprehensive Error Handling**
- ✅ **Rate Limiting** for stability
- ✅ **Consistent Response Format**

**Status**: API is production-ready and fully documented.
