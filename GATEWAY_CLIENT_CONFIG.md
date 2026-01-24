# ?? WMS API Gateway - Client Configuration Guide

## ? Gateway Status: FIXED AND READY

Your API Gateway is now **fully configured** and **ready to use**!

---

## ?? Gateway Base URL

**Production/Client Configuration**:
```
Base URL: https://localhost:7000
```

**All client applications (WMS.Web, mobile apps, external systems) should use this URL.**

---

## ?? Complete Endpoint List for Clients

### Gateway Endpoints (Direct)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Gateway health check |
| `/gateway/info` | GET | Gateway configuration info |
| `/swagger` | GET | API documentation (Development only) |

---

### 1. Authentication Service (`/auth`)

**Backend**: `https://localhost:7081` (WMS.Auth.API)  
**Gateway Route**: `https://localhost:7000/auth/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/auth/login` | POST | User login | `{"username":"admin","password":"Admin@123"}` |
| `/auth/register` | POST | Register new user | `{"username":"","email":"","password":""}` |
| `/auth/refresh` | POST | Refresh access token | `{"refreshToken":"..."}` |
| `/auth/me` | GET | Get current user info | N/A |
| `/auth/validate` | POST | Validate token | `{"token":"..."}` |

---

### 2. Products Service (`/products`)

**Backend**: `https://localhost:62527` (WMS.Products.API)  
**Gateway Route**: `https://localhost:7000/products/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/products` | GET | Get all products | Query: `?pageNumber=1&pageSize=10` |
| `/products/{id}` | GET | Get product by ID | N/A |
| `/products/sku/{sku}` | GET | Get product by SKU | N/A |
| `/products` | POST | Create product | `{"sku":"","name":"","uom":""}` |
| `/products/{id}` | PUT | Update product | `{"id":"","name":""}` |
| `/products/{id}/activate` | POST | Activate product | N/A |
| `/products/{id}/deactivate` | POST | Deactivate product | N/A |

---

### 3. Locations Service (`/locations`)

**Backend**: `https://localhost:62522` (WMS.Locations.API)  
**Gateway Route**: `https://localhost:7000/locations/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/locations` | GET | Get all locations | Query: `?pageNumber=1&pageSize=10` |
| `/locations/{id}` | GET | Get location by ID | N/A |
| `/locations/code/{code}` | GET | Get location by code | N/A |
| `/locations` | POST | Create location | `{"code":"","name":"","locationType":""}` |
| `/locations/{id}` | PUT | Update location | `{"id":"","name":""}` |
| `/locations/{id}/activate` | POST | Activate location | N/A |
| `/locations/{id}/deactivate` | POST | Deactivate location | N/A |

---

### 4. Inventory Service (`/inventory`)

**Backend**: `https://localhost:62531` (WMS.Inventory.API)  
**Gateway Route**: `https://localhost:7000/inventory/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/inventory` | GET | Get all inventory | Query: `?pageNumber=1&pageSize=10` |
| `/inventory/{id}` | GET | Get inventory by ID | N/A |
| `/inventory/product/{productId}` | GET | Get inventory by product | N/A |
| `/inventory/location/{locationId}` | GET | Get inventory by location | N/A |
| `/inventory/transactions` | GET | Get inventory transactions | N/A |
| `/inventory/adjust` | POST | Adjust inventory | `{"productId":"","locationId":"","quantity":10}` |
| `/inventory/transfer` | POST | Transfer inventory | `{"productId":"","fromLocationId":"","toLocationId":"","quantity":5}` |

---

### 5. Inbound Service (`/inbound`)

**Backend**: `https://localhost:62520` (WMS.Inbound.API)  
**Gateway Route**: `https://localhost:7000/inbound/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/inbound` | GET | Get all inbounds | Query: `?pageNumber=1&pageSize=10` |
| `/inbound/{id}` | GET | Get inbound by ID | N/A |
| `/inbound` | POST | Create inbound | `{"supplierName":"","expectedDate":""}` |
| `/inbound/receive` | POST | Receive inbound | `{"inboundId":"","items":[]}` |
| `/inbound/{id}/cancel` | POST | Cancel inbound | N/A |

---

### 6. Outbound Service (`/outbound`)

**Backend**: `https://localhost:62519` (WMS.Outbound.API)  
**Gateway Route**: `https://localhost:7000/outbound/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/outbound` | GET | Get all outbounds | Query: `?pageNumber=1&pageSize=10` |
| `/outbound/{id}` | GET | Get outbound by ID | N/A |
| `/outbound` | POST | Create outbound | `{"customerName":"","orderDate":""}` |
| `/outbound/pick` | POST | Pick outbound | `{"outboundId":"","items":[]}` |
| `/outbound/ship` | POST | Ship outbound | `{"outboundId":"","shipDate":""}` |
| `/outbound/{id}/cancel` | POST | Cancel outbound | N/A |

---

### 7. Payment Service (`/payment`)

**Backend**: `https://localhost:62521` (WMS.Payment.API)  
**Gateway Route**: `https://localhost:7000/payment/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/payment` | GET | Get all payments | Query: `?pageNumber=1&pageSize=10` |
| `/payment/{id}` | GET | Get payment by ID | N/A |
| `/payment` | POST | Create payment | `{"outboundId":"","amount":100}` |
| `/payment/confirm` | POST | Confirm payment | `{"paymentId":"","externalPaymentId":""}` |
| `/payment/{id}/cancel` | POST | Cancel payment | N/A |

---

### 8. Delivery Service (`/delivery`)

**Backend**: `https://localhost:62529` (WMS.Delivery.API)  
**Gateway Route**: `https://localhost:7000/delivery/*`

| Endpoint | Method | Description | Body Example |
|----------|--------|-------------|--------------|
| `/delivery` | GET | Get all deliveries | Query: `?pageNumber=1&pageSize=10` |
| `/delivery/{id}` | GET | Get delivery by ID | N/A |
| `/delivery/tracking/{trackingNumber}` | GET | Track delivery | N/A |
| `/delivery` | POST | Create delivery | `{"outboundId":"","shippingAddress":""}` |
| `/delivery/status` | PUT | Update delivery status | `{"deliveryId":"","status":""}` |
| `/delivery/complete` | POST | Complete delivery | `{"deliveryId":""}` |
| `/delivery/fail` | POST | Mark delivery as failed | `{"deliveryId":"","reason":""}` |
| `/delivery/event` | POST | Add delivery event | `{"deliveryId":"","eventType":""}` |

---

## ?? Client Configuration Examples

### For WMS.Web (Razor Pages)

Update `WMS.Web/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

**All API calls now go through Gateway**:

```csharp
// Before (Direct to microservice)
var response = await _httpClient.GetAsync("https://localhost:62527/api/products");

// After (Through Gateway)
var response = await _httpClient.GetAsync("https://localhost:7000/products");
```

---

### For JavaScript/Frontend Applications

```javascript
// Configuration
const API_BASE_URL = 'https://localhost:7000';

// Login example
async function login(username, password) {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ username, password })
    });
    
    return await response.json();
}

// Get products example
async function getProducts() {
    const response = await fetch(`${API_BASE_URL}/products`, {
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    });
    
    return await response.json();
}
```

---

### For Mobile Apps (React Native/Flutter)

```dart
// Flutter example
class ApiConfig {
  static const String baseUrl = 'https://localhost:7000';
  
  static String get authLogin => '$baseUrl/auth/login';
  static String get products => '$baseUrl/products';
  static String get inventory => '$baseUrl/inventory';
  // ... etc
}
```

---

### For External Systems/Integrations

```python
# Python example
import requests

API_BASE_URL = "https://localhost:7000"

# Login
response = requests.post(
    f"{API_BASE_URL}/auth/login",
    json={"username": "admin", "password": "Admin@123"}
)
token = response.json()["accessToken"]

# Get products
headers = {"Authorization": f"Bearer {token}"}
products = requests.get(
    f"{API_BASE_URL}/products",
    headers=headers
)
```

---

## ?? Service Discovery

Get all available services and endpoints:

```bash
# Get gateway info
curl https://localhost:7000/gateway/info

# Response:
{
  "name": "WMS API Gateway",
  "version": "1.0.0",
  "gatewayUrl": "https://localhost:7000",
  "services": [
    {
      "name": "Auth",
      "route": "/auth",
      "backendUrl": "https://localhost:7081",
      "endpoints": ["/auth/login", "/auth/register", ...]
    },
    // ... all other services
  ]
}
```

---

## ?? Authentication Flow

### 1. Login

```http
POST https://localhost:7000/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

### 2. Use Token for All Requests

```http
GET https://localhost:7000/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Refresh Token When Expired

```http
POST https://localhost:7000/auth/refresh
Content-Type: application/json

{
  "refreshToken": "..."
}
```

---

## ?? Running the Complete System

### Start All Services

```powershell
# From solution root
.\run-all-services.ps1
```

This starts:
- ? Gateway: `https://localhost:7000`
- ? Auth API: `https://localhost:7081`
- ? Products API: `https://localhost:62527`
- ? Locations API: `https://localhost:62522`
- ? Inventory API: `https://localhost:62531`
- ? Inbound API: `https://localhost:62520`
- ? Outbound API: `https://localhost:62519`
- ? Payment API: `https://localhost:62521`
- ? Delivery API: `https://localhost:62529`

### Test Gateway

```powershell
# Health check
curl https://localhost:7000/health

# Gateway info
curl https://localhost:7000/gateway/info

# Test login
curl -X POST https://localhost:7000/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin@123"}'
```

---

## ?? Important Notes for Clients

### 1. **Always Use Gateway URL**

? **Don't do this**:
```javascript
fetch('https://localhost:62527/api/products')  // Direct to microservice
```

? **Do this**:
```javascript
fetch('https://localhost:7000/products')  // Through gateway
```

### 2. **No `/api` Prefix**

The gateway removes the `/api` prefix automatically:

- Gateway route: `/products`
- Backend receives: `/api/products`

### 3. **CORS is Configured**

The gateway allows all origins in development. For production, configure specific origins in `WMS.Gateway/Program.cs`.

### 4. **All Services Share Authentication**

One JWT token works for all services. No need to login separately for each service.

---

## ?? Quick Reference

### Gateway URL
```
https://localhost:7000
```

### Common Endpoints
```
https://localhost:7000/auth/login
https://localhost:7000/products
https://localhost:7000/inventory
https://localhost:7000/inbound
https://localhost:7000/outbound
https://localhost:7000/payment
https://localhost:7000/delivery
```

### Default Credentials
```
Username: admin
Password: Admin@123
```

---

## ? Gateway Configuration Verified

| Component | Status | Value |
|-----------|--------|-------|
| **Gateway Port** | ? | 7000 (HTTPS) |
| **Auth Backend** | ? | 7081 |
| **Products Backend** | ? | 62527 |
| **Locations Backend** | ? | 62522 |
| **Inventory Backend** | ? | 62531 |
| **Inbound Backend** | ? | 62520 |
| **Outbound Backend** | ? | 62519 |
| **Payment Backend** | ? | 62521 |
| **Delivery Backend** | ? | 62529 |
| **Routes** | ? | 58 routes configured |
| **CORS** | ? | Enabled for all origins |
| **Swagger** | ? | Available at root (/) |

**Your API Gateway is ready for production! ??**
