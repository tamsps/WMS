# WMS - Complete API Reference Guide

**Version**: 1.0  
**Date**: January 28, 2026  
**Base URL**: `https://localhost:5000/api` (via Gateway)  
**Authentication**: JWT Bearer Token

---

## Table of Contents

1. [Authentication API](#authentication-api)
2. [Product API](#product-api)
3. [Location API](#location-api)
4. [Inbound API](#inbound-api)
5. [Outbound API](#outbound-api)
6. [Payment API](#payment-api)
7. [Delivery API](#delivery-api)
8. [Inventory API](#inventory-api)
9. [Common Response Formats](#common-response-formats)
10. [Error Handling](#error-handling)
11. [Rate Limiting](#rate-limiting)
12. [Postman Collection](#postman-collection)

---

## Authentication API

**Service Port**: 5002  
**Base URL**: `https://localhost:5000/api/auth`

### 1. User Login

Creates a JWT token for subsequent API calls.

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
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwidXNlcm5hbWUiOiJhZG1pbiIsIm5hbWUiOiJBZG1pbmlzdHJhdG9yIiwiaWF0IjoxNzA0NjA3ODAwLCJleHAiOjE3MDQ2MTE0MDB9.dummySignature",
    "refreshToken": "refresh_token_value...",
    "expiresIn": 3600,
    "user": {
      "id": "user_001",
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

### 2. Get Current User

Get information about the logged-in user.

```http
GET /api/auth/me
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "user_001",
    "username": "admin",
    "email": "admin@wms.local",
    "fullName": "Administrator",
    "roles": ["Admin", "Manager"],
    "lastLogin": "2024-01-28T14:30:00Z",
    "isActive": true
  }
}
```

### 3. Refresh Token

Get a new JWT token using the refresh token.

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "token": "expired_token_here",
  "refreshToken": "refresh_token_value"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "token": "new_jwt_token_here",
    "expiresIn": 3600
  }
}
```

### 4. Change Password

Change the password for the current user.

```http
POST /api/auth/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "oldPassword": "Admin@123",
  "newPassword": "NewPassword@456",
  "confirmPassword": "NewPassword@456"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

### 5. User Logout

```http
POST /api/auth/logout
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Logout successful"
}
```

---

## Product API

**Service Port**: 5003  
**Base URL**: `https://localhost:5000/api/products`

### 1. List All Products

Retrieve products with pagination and filtering.

```http
GET /api/products?pageNumber=1&pageSize=10&search=iPhone&isActive=true
Authorization: Bearer {token}
```

**Query Parameters**:
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `search`: Search by name or SKU
- `isActive`: Filter by status (true/false)

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "prod_001",
        "name": "iPhone 15",
        "sku": "SKU-001",
        "category": "Electronics",
        "description": "Latest iPhone model",
        "price": 999.99,
        "quantity": 150,
        "isActive": true,
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-28T14:30:00Z"
      }
    ],
    "totalCount": 245,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 25
  },
  "message": "Products retrieved successfully"
}
```

### 2. Get Product Details

```http
GET /api/products/{id}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "prod_001",
    "name": "iPhone 15",
    "sku": "SKU-001",
    "category": "Electronics",
    "description": "Latest iPhone model with advanced features",
    "price": 999.99,
    "quantity": 150,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-28T14:30:00Z",
    "createdBy": "admin",
    "updatedBy": "manager"
  }
}
```

### 3. Create Product

```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "iPhone 15",
  "sku": "SKU-001",
  "category": "Electronics",
  "description": "Latest iPhone model",
  "price": 999.99
}
```

**Response (201 Created)**:
```json
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

### 4. Update Product

```http
PUT /api/products/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "iPhone 15 Pro",
  "price": 1099.99,
  "category": "Electronics"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "prod_001",
    "name": "iPhone 15 Pro",
    "updatedAt": "2024-01-28T15:00:00Z"
  },
  "message": "Product updated successfully"
}
```

### 5. Delete Product

```http
DELETE /api/products/{id}
Authorization: Bearer {token}
```

**Response (204 No Content)**: No body returned

### 6. Activate Product

```http
PATCH /api/products/{id}/activate
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "prod_001",
    "isActive": true
  }
}
```

### 7. Deactivate Product

```http
PATCH /api/products/{id}/deactivate
Authorization: Bearer {token}
```

---

## Location API

**Service Port**: 5004  
**Base URL**: `https://localhost:5000/api/locations`

### 1. List All Locations

```http
GET /api/locations?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "loc_001",
        "code": "ZONE-A",
        "name": "Zone A",
        "locationType": "Zone",
        "capacity": 1000,
        "currentUtilization": 350,
        "parentLocationId": null,
        "description": "Main receiving zone",
        "isActive": true,
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "totalCount": 15,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

### 2. Get Location Tree (Hierarchical)

Get hierarchical structure of all locations.

```http
GET /api/locations/tree
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "id": "loc_001",
      "code": "ZONE-A",
      "name": "Zone A",
      "locationType": "Zone",
      "capacity": 1000,
      "currentUtilization": 350,
      "children": [
        {
          "id": "loc_002",
          "code": "RACK-A1",
          "name": "Rack A1",
          "locationType": "Rack",
          "capacity": 500,
          "currentUtilization": 200,
          "children": [
            {
              "id": "loc_003",
              "code": "BIN-A1-01",
              "name": "Bin A1-01",
              "locationType": "Bin",
              "capacity": 50,
              "currentUtilization": 45,
              "children": []
            }
          ]
        }
      ]
    }
  ]
}
```

### 3. Get Location Details

```http
GET /api/locations/{id}
Authorization: Bearer {token}
```

### 4. Create Location

```http
POST /api/locations
Authorization: Bearer {token}
Content-Type: application/json

{
  "code": "ZONE-A",
  "name": "Zone A",
  "locationType": "Zone",
  "capacity": 1000,
  "parentLocationId": null,
  "description": "Main receiving zone"
}
```

### 5. Update Location

```http
PUT /api/locations/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Zone A - Updated",
  "capacity": 1200
}
```

### 6. Delete Location

```http
DELETE /api/locations/{id}
Authorization: Bearer {token}
```

### 7. Get Location Inventory

```http
GET /api/locations/{id}/inventory
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "locationId": "loc_001",
    "locationName": "Zone A",
    "items": [
      {
        "productId": "prod_001",
        "productName": "iPhone 15",
        "sku": "SKU-001",
        "quantity": 100,
        "unit": "pcs"
      }
    ],
    "totalItems": 5,
    "totalQuantity": 350
  }
}
```

---

## Inbound API

**Service Port**: 5005  
**Base URL**: `https://localhost:5000/api/inbound`

### 1. List Inbound Orders

```http
GET /api/inbound?pageNumber=1&pageSize=10&status=Draft
Authorization: Bearer {token}
```

**Query Parameters**:
- `pageNumber`: Page number
- `pageSize`: Items per page
- `status`: Draft, Receiving, Completed, Cancelled
- `search`: Search by reference number

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "inb_001",
        "referenceNumber": "INB-2024-001",
        "supplierName": "Supplier A",
        "expectedDeliveryDate": "2024-02-15",
        "status": "Draft",
        "totalItems": 5,
        "totalExpectedQuantity": 500,
        "totalReceivedQuantity": 0,
        "progressPercentage": 0,
        "createdAt": "2024-01-28T14:30:00Z"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

### 2. Get Inbound Order Details

```http
GET /api/inbound/{id}
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "inb_001",
    "referenceNumber": "INB-2024-001",
    "supplierId": "supp_001",
    "supplierName": "Supplier A",
    "expectedDeliveryDate": "2024-02-15",
    "status": "Draft",
    "notes": "Rush delivery",
    "items": [
      {
        "id": "inb_item_001",
        "productId": "prod_001",
        "productName": "iPhone 15",
        "productSku": "SKU-001",
        "expectedQuantity": 100,
        "receivedQuantity": 0,
        "unit": "pcs"
      }
    ],
    "createdAt": "2024-01-28T14:30:00Z",
    "createdBy": "admin"
  }
}
```

### 3. Create Inbound Order

```http
POST /api/inbound
Authorization: Bearer {token}
Content-Type: application/json

{
  "referenceNumber": "INB-2024-001",
  "supplierId": "supp_001",
  "expectedDeliveryDate": "2024-02-15",
  "items": [
    {
      "productId": "prod_001",
      "expectedQuantity": 100,
      "unit": "pcs"
    }
  ],
  "notes": "Rush delivery"
}
```

### 4. Start Receiving Items

```http
POST /api/inbound/{id}/receive
Authorization: Bearer {token}
Content-Type: application/json

{
  "items": [
    {
      "itemId": "inb_item_001",
      "receivedQuantity": 50,
      "locationId": "loc_001"
    }
  ],
  "receivingNotes": "Quality check passed"
}
```

### 5. Complete Inbound Order

```http
POST /api/inbound/{id}/complete
Authorization: Bearer {token}
```

### 6. Cancel Inbound Order

```http
DELETE /api/inbound/{id}
Authorization: Bearer {token}
```

---

## Outbound API

**Service Port**: 5006  
**Base URL**: `https://localhost:5000/api/outbound`

### 1. List Outbound Orders

```http
GET /api/outbound?pageNumber=1&pageSize=10&status=Draft
Authorization: Bearer {token}
```

### 2. Get Outbound Order Details

```http
GET /api/outbound/{id}
Authorization: Bearer {token}
```

### 3. Create Outbound Order

```http
POST /api/outbound
Authorization: Bearer {token}
Content-Type: application/json

{
  "referenceNumber": "ORD-2024-001",
  "customerId": "cust_001",
  "customerName": "Acme Corp",
  "orderDate": "2024-01-28",
  "shippingAddress": "123 Main St, New York, NY 10001",
  "items": [
    {
      "productId": "prod_001",
      "orderedQuantity": 50
    }
  ],
  "notes": "Urgent delivery"
}
```

### 4. Ship Order

```http
POST /api/outbound/{id}/ship
Authorization: Bearer {token}
Content-Type: application/json

{
  "trackingNumber": "FDX-123456789",
  "shippedDate": "2024-01-28",
  "items": [
    {
      "itemId": "outb_item_001",
      "shipQuantity": 50
    }
  ]
}
```

### 5. Cancel Order

```http
DELETE /api/outbound/{id}
Authorization: Bearer {token}
```

---

## Payment API

**Service Port**: 5007  
**Base URL**: `https://localhost:5000/api/payment`

### 1. List Payments

```http
GET /api/payment?pageNumber=1&pageSize=10&status=Pending
Authorization: Bearer {token}
```

### 2. Get Payment Details

```http
GET /api/payment/{id}
Authorization: Bearer {token}
```

### 3. Create Payment

```http
POST /api/payment
Authorization: Bearer {token}
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
```

**Supported Payment Methods**:
- Cash
- Credit Card
- Bank Transfer
- Digital Wallet
- Check

**Supported Currencies**:
- USD (US Dollar)
- EUR (Euro)
- GBP (British Pound)
- JPY (Japanese Yen)
- CNY (Chinese Yuan)
- IDR (Indonesian Rupiah)

### 4. Confirm Payment

```http
POST /api/payment/{id}/confirm
Authorization: Bearer {token}
```

### 5. Cancel/Refund Payment

```http
DELETE /api/payment/{id}
Authorization: Bearer {token}
```

---

## Delivery API

**Service Port**: 5009  
**Base URL**: `https://localhost:5000/api/delivery`

### 1. List Deliveries (Admin Only)

```http
GET /api/delivery?pageNumber=1&pageSize=10&status=In Transit
Authorization: Bearer {token}
```

### 2. Get Delivery Details (Admin Only)

```http
GET /api/delivery/{id}
Authorization: Bearer {token}
```

### 3. Create Delivery

```http
POST /api/delivery
Authorization: Bearer {token}
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
```

**Supported Carriers**:
- FedEx
- UPS
- DHL
- USPS
- Local Courier

### 4. Update Delivery Status

```http
PUT /api/delivery/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "In Transit",
  "location": "Distribution Center, Chicago IL",
  "updateNotes": "Package in transit"
}
```

**Valid Status Values**:
- Pending
- In Transit
- Out for Delivery
- Delivered
- Failed
- Returned

### 5. Public Tracking (No Authentication)

Get tracking information without login.

```http
GET /api/delivery/track/{trackingNumber}
```

**Example**:
```http
GET /api/delivery/track/FDX-123456789
```

**Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "trackingNumber": "FDX-123456789",
    "status": "In Transit",
    "carrier": "FedEx",
    "recipientName": "John Doe",
    "recipientPhone": "+1-555-0123",
    "deliveryAddress": "123 Main St, New York, NY 10001",
    "estimatedDeliveryDate": "2024-02-02",
    "currentLocation": "Distribution Center, Chicago IL",
    "lastUpdate": "2024-01-29T08:15:00Z",
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

## Inventory API

**Service Port**: 5010  
**Base URL**: `https://localhost:5000/api/inventory`

### 1. Get Inventory Overview

```http
GET /api/inventory?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

**Response (200 OK)**:
```json
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
    "totalCount": 125,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

### 2. Get Product Inventory

```http
GET /api/inventory/{productId}
Authorization: Bearer {token}
```

### 3. Get Location Inventory

```http
GET /api/inventory/location/{locationId}
Authorization: Bearer {token}
```

### 4. Get Transaction History

```http
GET /api/inventory/transactions?productId=prod_001&days=30
Authorization: Bearer {token}
```

**Query Parameters**:
- `productId`: Filter by product (optional)
- `days`: Last N days (default: 30)
- `type`: Transaction type (Inbound, Outbound, Adjustment)

**Response (200 OK)**:
```json
{
  "success": true,
  "data": [
    {
      "id": "txn_001",
      "type": "Inbound",
      "productId": "prod_001",
      "productName": "iPhone 15",
      "quantity": 100,
      "fromLocation": null,
      "toLocation": "loc_001",
      "referenceId": "inb_001",
      "timestamp": "2024-01-28T14:30:00Z"
    }
  ],
  "totalCount": 45
}
```

### 5. Manual Inventory Adjustment

```http
POST /api/inventory/adjust
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "prod_001",
  "locationId": "loc_001",
  "adjustmentQuantity": 10,
  "reason": "Inventory count discrepancy",
  "notes": "Physical count verification"
}
```

### 6. Get Low Stock Alerts

```http
GET /api/inventory/alerts
Authorization: Bearer {token}
```

---

## Common Response Formats

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "resource_id",
    "name": "Resource Name"
  }
}
```

### List Response with Pagination
```json
{
  "success": true,
  "message": "Items retrieved successfully",
  "data": {
    "items": [],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "errors": [
    "Field is required",
    "Invalid format"
  ]
}
```

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Successful GET/PUT/PATCH |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Invalid input parameters |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists |
| 500 | Server Error | Internal server error |

### Error Response Example
```json
{
  "success": false,
  "message": "Validation error",
  "errors": [
    "Product name is required",
    "SKU must be unique",
    "Price must be greater than 0"
  ]
}
```

---

## Rate Limiting

Current limits (subject to change):
- **Authentication endpoints**: 5 requests/minute per IP
- **Read operations**: 100 requests/minute per user
- **Write operations**: 20 requests/minute per user

Rate limit headers in response:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1704067200
```

---

## Postman Collection

### Setup Postman

1. **Create Environment** in Postman:
   ```json
   {
     "name": "WMS Local",
     "values": [
       {
         "key": "baseUrl",
         "value": "https://localhost:5000/api",
         "enabled": true
       },
       {
         "key": "token",
         "value": "",
         "enabled": true
       }
     ]
   }
   ```

2. **Login Request** (Get Token):
   ```
   POST {{baseUrl}}/auth/login
   Content-Type: application/json

   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
   - In **Tests** tab, add:
   ```javascript
   var jsonData = pm.response.json();
   pm.environment.set("token", jsonData.data.token);
   ```

3. **Use Token** in subsequent requests:
   ```
   Authorization: Bearer {{token}}
   ```

---

## Testing with curl

### Login and Get Token
```bash
curl -k -X POST https://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}' \
  | jq '.data.token' > token.txt
```

### Get Products
```bash
TOKEN=$(cat token.txt | tr -d '"')
curl -k -X GET https://localhost:5000/api/products \
  -H "Authorization: Bearer $TOKEN"
```

### Create Product
```bash
TOKEN=$(cat token.txt | tr -d '"')
curl -k -X POST https://localhost:5000/api/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "sku": "TEST-001",
    "price": 99.99
  }'
```

---

## API Versioning

Current API version: **v1**

Future versions may be accessed via:
```
https://localhost:5000/api/v2/...
```

---

## Webhooks (Future Feature)

Webhooks for real-time events are planned:
- `order.created`
- `shipment.updated`
- `payment.confirmed`
- `inventory.low_stock`

---

## SDK Integration (Future)

Official SDKs planned for:
- .NET/C#
- JavaScript/TypeScript
- Python
- Java

---

## Support & Documentation

- **Full Documentation**: ARCHITECTURE_AND_DEPLOYMENT_GUIDE.md
- **Quick Start**: QUICK_START_FINAL.md
- **API Base URL**: https://localhost:5000/swagger
- **Gateway Health**: https://localhost:5000/health

---

**Last Updated**: January 28, 2026  
**Version**: 1.0  
**Status**: Production Ready ✅
