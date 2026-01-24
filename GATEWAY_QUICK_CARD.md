# ?? API Gateway - Quick Reference Card

## ? Status: FIXED & READY

---

## ?? Gateway URL
https://localhost:7000
**Use this URL for ALL API calls from ANY client!**

---

## ?? Quick Test Commands

### Health Checkcurl https://localhost:7000/health
### Gateway Infocurl https://localhost:7000/gateway/info
### Logincurl -X POST https://localhost:7000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
### Get Productscurl https://localhost:7000/products \
  -H "Authorization: Bearer YOUR_TOKEN"
---

## ?? All Services

| Service | Gateway Route | Backend Port |
|---------|---------------|--------------|
| **Auth** | `/auth/*` | 7081 |
| **Products** | `/products/*` | 62527 |
| **Locations** | `/locations/*` | 62522 |
| **Inventory** | `/inventory/*` | 62531 |
| **Inbound** | `/inbound/*` | 62520 |
| **Outbound** | `/outbound/*` | 62519 |
| **Payment** | `/payment/*` | 62521 |
| **Delivery** | `/delivery/*` | 62529 |

---

## ?? Client Configuration

### WMS.Web (appsettings.json){
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000"
  }
}
### JavaScriptconst API_URL = 'https://localhost:7000';
### Mobile Appsstatic const String baseUrl = 'https://localhost:7000';
---

## ?? Start System
.\run-all-services.ps1
---

## ?? Key Endpoints

### AuthenticationPOST /auth/login
POST /auth/register
POST /auth/refresh
GET  /auth/me
### ProductsGET  /products
POST /products
PUT  /products/{id}
### InventoryGET  /inventory
POST /inventory/adjust
POST /inventory/transfer
### OrdersGET  /inbound
POST /inbound
GET  /outbound
POST /outbound
---

## ? Build Status

- [x] Gateway: ? Built successfully
- [x] All APIs: ? Built successfully
- [x] Routes: ? 58 routes configured
- [x] Ports: ? All verified

---

## ?? Documentation

- `GATEWAY_FIXED_SUMMARY.md` - Complete fix summary
- `GATEWAY_CLIENT_CONFIG.md` - Full client guide
- `WMS.Gateway/appsettings.json` - Configuration

---

**Your API Gateway is production ready! ??**
