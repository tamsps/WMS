# ?? WMS API Gateway - Quick Reference Card

**Port:** 5000 | **Technology:** YARP | **Status:** ? Production Ready

---

## ? Quick Start

```powershell
# Start everything with gateway
./run-with-gateway.ps1

# Or start gateway only
cd WMS.Gateway
dotnet run
```

**Access:** https://localhost:5000

---

## ??? Route Mapping

| Request | Gateway Route | Service |
|---------|---------------|---------|
| `GET /auth/auth/login` | `/auth/*` | Auth API (5001) |
| `GET /products/products` | `/products/*` | Products API (5002) |
| `GET /locations/location` | `/locations/*` | Locations API (5003) |
| `GET /inbound/inbound` | `/inbound/*` | Inbound API (5004) |
| `GET /outbound/outbound` | `/outbound/*` | Outbound API (5005) |
| `GET /inventory/inventory` | `/inventory/*` | Inventory API (5006) |
| `GET /payment/payment` | `/payment/*` | Payment API (5007) |
| `GET /delivery/delivery` | `/delivery/*` | Delivery API (5008) |

---

## ?? Gateway Endpoints

```bash
# Swagger UI
https://localhost:5000

# Health check
https://localhost:5000/health

# Service info
https://localhost:5000/gateway/info
```

---

## ?? Example Usage

### 1. Login
```bash
curl -X POST https://localhost:5000/auth/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### 2. Get Products
```bash
curl https://localhost:5000/products/products \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 3. Create Inbound
```bash
curl -X POST https://localhost:5000/inbound/inbound \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"referenceNumber":"INB-001","supplierName":"ABC",...}'
```

---

## ?? Configuration

**File:** `WMS.Gateway/appsettings.json`

Change backend URLs:
```json
{
  "ReverseProxy": {
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "auth-api": {
            "Address": "https://localhost:5001"
          }
        }
      }
    }
  }
}
```

---

## ??? Architecture

```
Client
  ?
Gateway (5000)
  ??? Auth (5001)
  ??? Products (5002)
  ??? Locations (5003)
  ??? Inbound (5004)
  ??? Outbound (5005)
  ??? Inventory (5006)
  ??? Payment (5007)
  ??? Delivery (5008)
```

---

## ?? Path Transformation

```
Request:  /products/products
           ? Transform
Backend:  /api/products
```

---

## ?? Documentation

- **Complete Guide:** `API_GATEWAY_GUIDE.md`
- **Implementation:** `GATEWAY_IMPLEMENTATION_SUMMARY.md`
- **Microservices:** `README_MICROSERVICES.md`

---

## ??? Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 5000 in use | Change port in `launchSettings.json` |
| 502 Bad Gateway | Check backend service is running |
| SSL errors | Run `dotnet dev-certs https --trust` |
| Can't reach service | Verify backend URL in `appsettings.json` |

---

## ? Verification

```powershell
# Check gateway health
curl https://localhost:5000/health

# List all services
curl https://localhost:5000/gateway/info

# Test login
curl -X POST https://localhost:5000/auth/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

---

**Created:** January 23, 2026 | **Version:** 1.0.0 | **YARP:** 2.3.0
