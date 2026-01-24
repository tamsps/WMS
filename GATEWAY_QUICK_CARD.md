# ?? Gateway API - Quick Reference Card

## ?? Gateway Base URL
```
https://localhost:7000
```

---

## ?? All Endpoints at a Glance

### ?? Auth (7081)
```
POST   /auth/login              - Login
POST   /auth/register           - Register
POST   /auth/refresh            - Refresh token
GET    /auth/me                 - Get profile
GET    /auth/validate           - Validate token
```

### ?? Products (62527)
```
GET    /products                - List products
GET    /products/{id}           - Get product
GET    /products/sku/{sku}      - Get by SKU
POST   /products                - Create product
PUT    /products/{id}           - Update product
PATCH  /products/{id}/activate  - Activate
PATCH  /products/{id}/deactivate - Deactivate
```

### ?? Locations (62522)
```
GET    /locations                  - List locations
GET    /locations/{id}             - Get location
GET    /locations/code/{code}      - Get by code
POST   /locations                  - Create location
PUT    /locations/{id}             - Update location
PATCH  /locations/{id}/activate    - Activate
PATCH  /locations/{id}/deactivate  - Deactivate
```

### ?? Inventory (62531)
```
GET    /inventory                     - List inventory
GET    /inventory/{id}                - Get inventory
GET    /inventory/product/{productId} - By product
GET    /inventory/location/{locationId} - By location
GET    /inventory/transactions        - Transactions
POST   /inventory/adjust              - Adjust
POST   /inventory/transfer            - Transfer
```

### ?? Inbound (62520)
```
GET    /inbound            - List inbounds
GET    /inbound/{id}       - Get inbound
POST   /inbound            - Create inbound
POST   /inbound/receive    - Receive items
POST   /inbound/{id}/cancel - Cancel
```

### ?? Outbound (62519)
```
GET    /outbound            - List outbounds
GET    /outbound/{id}       - Get outbound
POST   /outbound            - Create outbound
POST   /outbound/pick       - Pick items
POST   /outbound/ship       - Ship
POST   /outbound/{id}/cancel - Cancel
```

### ?? Payment (62521)
```
GET    /payment            - List payments
GET    /payment/{id}       - Get payment
POST   /payment            - Create payment
POST   /payment/confirm    - Confirm
POST   /payment/{id}/cancel - Cancel
```

### ?? Delivery (62529)
```
GET    /delivery                        - List deliveries
GET    /delivery/{id}                   - Get delivery
GET    /delivery/tracking/{trackingNumber} - Track (public)
POST   /delivery                        - Create delivery
PUT    /delivery/status                 - Update status
POST   /delivery/complete               - Complete
POST   /delivery/fail                   - Mark failed
POST   /delivery/event                  - Add event
```

---

## ?? Authentication Header

```bash
Authorization: Bearer YOUR_ACCESS_TOKEN
```

---

## ?? Common Workflows

### 1. Login & Get Token
```bash
curl -X POST https://localhost:7000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### 2. Create Product
```bash
curl -X POST https://localhost:7000/products \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sku":"PROD001","name":"Product 1","uom":"PCS"}'
```

### 3. Receive Inbound
```bash
curl -X POST https://localhost:7000/inbound/receive \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"inboundId":"ID","items":[{"inboundItemId":"ITEM_ID","receivedQuantity":100}]}'
```

### 4. Ship Outbound
```bash
curl -X POST https://localhost:7000/outbound/ship \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"outboundId":"ID"}'
```

### 5. Track Delivery (No Auth)
```bash
curl https://localhost:7000/delivery/tracking/DEL-20240124-0001
```

---

## ?? Quick Start

```powershell
# Start gateway
cd WMS.Gateway
dotnet run

# Start all services
.\run-all-services.ps1

# Test
curl https://localhost:7000/auth/validate
```

---

## ?? Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 404 | Not Found |
| 500 | Server Error |

---

**Gateway Port:** 7000  
**All Routes:** 60+ endpoints  
**Services:** 8 microservices  
**Status:** ? Ready
