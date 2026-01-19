# Warehouse Management System (WMS)

A comprehensive Warehouse Management System built with .NET 9 following Clean Architecture principles.

## 🏗️ Architecture Overview

This WMS follows **Clean Architecture** with clear separation of concerns:

### Project Structure

```
WMS/
├── WMS.Domain/              # Enterprise Business Rules
│   ├── Entities/           # Domain entities
│   ├── Enums/              # Domain enumerations
│   ├── Common/             # Base entities and interfaces
│   └── Interfaces/         # Repository interfaces
├── WMS.Application/         # Application Business Rules
│   ├── DTOs/               # Data Transfer Objects
│   ├── Interfaces/         # Service interfaces
│   └── Common/             # Common models (Result, PagedResult)
├── WMS.Infrastructure/      # External Concerns
│   ├── Data/               # DbContext and configurations
│   ├── Repositories/       # Repository implementations
│   └── Services/           # Service implementations
├── WMS.API/                # API Layer (Presentation)
│   └── Controllers/        # API Controllers
└── WMS.Web/                # MVC Web App (Separate project)
```

## 📋 System Modules

### 1. Product (SKU) Management
- Create, Read, Update products
- Immutable SKU identifier
- Product activation/deactivation
- Only active products participate in transactions

### 2. Warehouse Location Management
- Hierarchical location structure
- Zone, Aisle, Rack, Shelf, Bin organization
- Capacity enforcement
- Location activation/deactivation

### 3. Inbound Processing
- Receive goods into warehouse
- Atomic transaction processing
- Inventory increment
- Put-away operations

### 4. Outbound Processing
- Pick and ship goods
- Prevent negative inventory
- Concurrent request handling
- Inventory deduction

### 5. Inventory Management
- Real-time stock visibility by SKU and location
- Inventory transactions audit trail
- Available quantity tracking
- Reserved quantity management

### 6. Payment Management
- Operational payment state control
- Prepaid, COD, Postpaid support
- Shipment gating logic
- Payment gateway integration ready
- Asynchronous webhook processing

### 7. Delivery & Shipment Management
- Physical shipment tracking
- Carrier and tracking number management
- Delivery status updates
- Failure and return handling
- Delivery events audit trail

## 🔐 Security

- **JWT Authentication** - Token-based authentication
- **Role-Based Access Control (RBAC)** - Admin, Manager, WarehouseStaff
- **Secure Password Hashing** - BCrypt implementation

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Database Setup

1. **Update Connection String** in `WMS.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

2. **Create Initial Migration**:

```powershell
# Navigate to API project
cd WMS.API

# Add Entity Framework tools (if not installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Running the API

```powershell
# Navigate to API project
cd WMS.API

# Run the application
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/` (Development only)

### Default Credentials

```
Username: admin
Password: Admin@123
```

## 📚 API Documentation

### Authentication

**Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-token",
  "expiresAt": "2026-01-17T12:00:00Z",
  "user": {
    "id": "guid",
    "username": "admin",
    "email": "admin@wms.com",
    "roles": ["Admin"]
  }
}
```

### Products API

**Get All Products**
```http
GET /api/products?pageNumber=1&pageSize=10&searchTerm=SKU001
Authorization: Bearer {token}
```

**Create Product**
```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "sku": "SKU001",
  "name": "Product Name",
  "description": "Description",
  "uom": "EA",
  "weight": 1.5,
  "length": 10,
  "width": 5,
  "height": 3
}
```

### Locations API

**Create Location**
```http
POST /api/locations
Authorization: Bearer {token}
Content-Type: application/json

{
  "code": "A-01-01-01",
  "name": "Aisle A Rack 1 Shelf 1",
  "zone": "A",
  "aisle": "01",
  "rack": "01",
  "shelf": "01",
  "capacity": 1000
}
```

### Inbound API

**Create Inbound**
```http
POST /api/inbound
Authorization: Bearer {token}
Content-Type: application/json

{
  "referenceNumber": "PO-001",
  "expectedDate": "2026-01-20",
  "supplierName": "Supplier ABC",
  "items": [
    {
      "productId": "guid",
      "locationId": "guid",
      "expectedQuantity": 100
    }
  ]
}
```

**Receive Inbound**
```http
POST /api/inbound/{id}/receive
Authorization: Bearer {token}
Content-Type: application/json

{
  "inboundId": "guid",
  "items": [
    {
      "inboundItemId": "guid",
      "receivedQuantity": 100,
      "damagedQuantity": 0
    }
  ]
}
```

### Outbound API

**Create Outbound**
```http
POST /api/outbound
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderNumber": "SO-001",
  "customerName": "Customer XYZ",
  "shippingAddress": "123 Main St",
  "items": [
    {
      "productId": "guid",
      "locationId": "guid",
      "orderedQuantity": 50
    }
  ]
}
```

**Ship Outbound**
```http
POST /api/outbound/{id}/ship
Authorization: Bearer {token}
```

## 🔄 Business Process Flow

### End-to-End Flow

1. **Master Data Setup**
   - Create products (SKUs)
   - Create warehouse locations
   - Ensure all master data exists

2. **Inbound Processing**
   - Goods arrive
   - Create inbound order
   - Receive and validate goods
   - Inventory increases atomically

3. **Outbound Processing**
   - Create outbound order
   - Validate availability
   - Pick items
   - Ship confirmation
   - Inventory decreases atomically

4. **Payment Handling**
   - For prepaid: require payment before shipment
   - For COD: allow shipment, payment on delivery
   - For postpaid: allow shipment, invoice later

5. **Delivery Execution**
   - Track delivery status
   - Handle delivery failures
   - Process returns via inbound flow

6. **Inventory Monitoring**
   - Real-time inventory visibility
   - Transaction audit trail
   - Reconciliation capabilities

## 🧪 Testing

### Sample API Test Flow

1. **Login**
2. **Create a Product** (SKU: TEST-001)
3. **Create a Location** (Code: A-01-01)
4. **Create Inbound** (Receive 100 units)
5. **Receive Inbound** (Confirm receipt)
6. **Check Inventory** (Should show 100 units)
7. **Create Outbound** (Ship 30 units)
8. **Ship Outbound** (Confirm shipment)
9. **Check Inventory** (Should show 70 units)

## 📊 Database Schema

Key entities and relationships:
- **Product** ← **Inventory** → **Location**
- **Inbound** → **InboundItem** → **Product**, **Location**
- **Outbound** → **OutboundItem** → **Product**, **Location**
- **Outbound** → **Payment** (1:1)
- **Outbound** → **Delivery** (1:1)
- **InventoryTransaction** → Audit trail for all inventory changes

## 🔧 Configuration

### JWT Settings

Update in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
  "Issuer": "WMS.API",
  "Audience": "WMS.Client",
  "ExpirationMinutes": 60
}
```

### CORS Settings

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:3000"
  ]
}
```

## 🎯 Key Features

✅ Clean Architecture with clear separation of concerns  
✅ JWT Authentication and Authorization  
✅ Role-Based Access Control (RBAC)  
✅ Atomic inventory transactions  
✅ Concurrent request handling  
✅ Negative inventory prevention  
✅ Complete audit trail  
✅ Payment state management  
✅ Delivery tracking  
✅ Swagger/OpenAPI documentation  
✅ Repository pattern with Unit of Work  
✅ Comprehensive error handling  
✅ Pagination support  
✅ Search and filtering  

## 📦 NuGet Packages Used

### WMS.Infrastructure
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)

### WMS.Application
- FluentValidation (12.1.1)
- FluentValidation.DependencyInjectionExtensions (12.1.1)

### WMS.API
- Swashbuckle.AspNetCore (latest)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)

## 🚧 Future Enhancements

- [ ] Batch picking operations
- [ ] Wave picking
- [ ] Cycle counting
- [ ] Barcode scanning integration
- [ ] Mobile app for warehouse staff
- [ ] Advanced reporting and analytics
- [ ] Integration with ERP systems
- [ ] 3PL integration
- [ ] Real-time notifications
- [ ] Multi-warehouse support

## 📝 License

This project is created for educational purposes.

## 👥 Support

For issues and questions, please refer to the documentation or create an issue in the repository.

---

**Built with ❤️ using .NET 9 and Clean Architecture**
