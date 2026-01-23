# ?? WMS - Warehouse Management System (Microservices Edition)

> A complete warehouse management solution built with .NET 9.0 microservices architecture

## ?? Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Microservices](#microservices)
- [Documentation](#documentation)
- [Running the Application](#running-the-application)
- [Development](#development)
- [Deployment](#deployment)

---

## ?? Overview

WMS has been refactored from a monolithic architecture to a **microservices architecture** with 8 independent services:

### What's New in v2.0?
- ? **8 Independent Microservices** - Each service can be deployed independently
- ? **Improved Scalability** - Scale services based on demand
- ? **Better Fault Isolation** - Service failures don't cascade
- ? **Faster Development** - Smaller codebases, faster builds
- ? **Technology Flexibility** - Each service can evolve independently

---

## ? Quick Start

### For Impatient Developers

```powershell
# 1. Build everything
dotnet build

# 2. Run all services (Windows)
.\run-all-services.ps1

# 3. Open browser to any service
# Auth: https://localhost:5001
# Products: https://localhost:5002
# etc...
```

?? **Detailed Guide**: See [QUICKSTART.md](QUICKSTART.md)

---

## ??? Architecture

```
????????????????????????????????????????????????????????
?                  WMS Web Application                  ?
?                  (Port: 5000)                         ?
????????????????????????????????????????????????????????
                      ?
         ???????????????????????????
         ?                         ?
    ????????????          ??????????????????
    ?  Auth    ?          ?   Products     ?
    ?  :5001   ?          ?   :5002        ?
    ????????????          ??????????????????
         ?                         ?
    ????????????          ??????????????????
    ?Locations ?          ?   Inventory    ?
    ?  :5003   ?          ?   :5004        ?
    ????????????          ??????????????????
         ?                         ?
    ????????????          ??????????????????
    ? Inbound  ?          ?   Outbound     ?
    ?  :5005   ?          ?   :5006        ?
    ????????????          ??????????????????
         ?                         ?
    ????????????          ??????????????????
    ? Payment  ?          ?   Delivery     ?
    ?  :5007   ?          ?   :5008        ?
    ????????????          ??????????????????
                      ?
         ???????????????????????????
         ?   SQL Server Database    ?
         ?       (WMSDB)            ?
         ????????????????????????????
```

?? **Full Architecture**: See [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md)

---

## ??? Microservices

| Service | Port | Responsibility | Swagger UI |
|---------|------|----------------|------------|
| **Auth** | 5001 | Authentication & Authorization | [Open](https://localhost:5001) |
| **Products** | 5002 | Product Catalog Management | [Open](https://localhost:5002) |
| **Locations** | 5003 | Warehouse Location Hierarchy | [Open](https://localhost:5003) |
| **Inventory** | 5004 | Stock Levels & Transactions | [Open](https://localhost:5004) |
| **Inbound** | 5005 | Receiving Operations | [Open](https://localhost:5005) |
| **Outbound** | 5006 | Shipping Operations | [Open](https://localhost:5006) |
| **Payment** | 5007 | Payment Tracking | [Open](https://localhost:5007) |
| **Delivery** | 5008 | Delivery Tracking | [Open](https://localhost:5008) |

### Default Credentials
- **Username**: `admin`
- **Password**: `Admin@123`

---

## ?? Documentation

| Document | Description |
|----------|-------------|
| [QUICKSTART.md](QUICKSTART.md) | Get started in 3 steps |
| [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md) | Complete architecture guide |
| [RUN_MICROSERVICES.md](RUN_MICROSERVICES.md) | Detailed running instructions |
| [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) | Complete refactoring summary |
| [USER_GUIDE.md](USER_GUIDE.md) | User guide with API reference |

---

## ?? Running the Application

### Option 1: PowerShell Script (Recommended)
```powershell
.\run-all-services.ps1
```

### Option 2: Docker Compose
```bash
docker-compose up
```

### Option 3: Visual Studio
1. Right-click solution ? Properties
2. Multiple startup projects
3. Set all `WMS.*.API` to Start
4. Press F5

### Option 4: Individual Services
```powershell
# Terminal 1
cd WMS.Auth.API
dotnet run --urls=https://localhost:5001

# Terminal 2
cd WMS.Products.API
dotnet run --urls=https://localhost:5002

# ... repeat for all services
```

?? **Detailed Instructions**: See [RUN_MICROSERVICES.md](RUN_MICROSERVICES.md)

---

## ??? Development

### Tech Stack
- **.NET 9.0** - Runtime framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Database
- **JWT** - Authentication
- **Swagger** - API documentation

### Project Structure
```
WMS/
??? WMS.Auth.API/              # Authentication microservice
??? WMS.Products.API/          # Products microservice
??? WMS.Locations.API/         # Locations microservice
??? WMS.Inventory.API/         # Inventory microservice
??? WMS.Inbound.API/           # Inbound microservice
??? WMS.Outbound.API/          # Outbound microservice
??? WMS.Payment.API/           # Payment microservice
??? WMS.Delivery.API/          # Delivery microservice
??? WMS.Web/                   # Web application
??? WMS.Domain/                # Shared domain models
??? WMS.Application/           # Shared application layer
??? WMS.Infrastructure/        # Shared infrastructure
??? docker-compose.yml         # Docker orchestration
??? *.md                       # Documentation files
```

### Building
```powershell
# Build all projects
dotnet build

# Build specific project
dotnet build WMS.Auth.API/WMS.Auth.API.csproj

# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Testing
```powershell
# Run tests (when implemented)
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## ?? Deployment

### Docker
```bash
# Build images
docker-compose build

# Run services
docker-compose up

# Run in background
docker-compose up -d

# Stop services
docker-compose down
```

### Kubernetes (Future)
```bash
kubectl apply -f k8s/
```

### Azure (Future)
```bash
az containerapp up
```

---

## ?? Features

### Core Features
- ? User Authentication & Authorization
- ? Product Management
- ? Location Management
- ? Inventory Tracking
- ? Inbound Processing
- ? Outbound Processing
- ? Payment Management
- ? Delivery Tracking

### Technical Features
- ? JWT Authentication
- ? Role-Based Access Control
- ? RESTful API Design
- ? Swagger Documentation
- ? CORS Enabled
- ? Microservices Architecture
- ? Shared Database
- ? Docker Support

---

## ?? Testing

### Manual Testing
1. Start all services
2. Navigate to Swagger UI
3. Login to get JWT token
4. Test endpoints with token

### API Testing
```bash
# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'

# Get products (with token)
curl -X GET https://localhost:5002/api/products \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ?? Configuration

### Database
Edit `appsettings.json` in each service:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;..."
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKey...",
    "Issuer": "WMS.ServiceName.API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60
  }
}
```

---

## ?? Contributing

### Setup Scripts
- `setup-microservices-complete.ps1` - Initial setup
- `generate-project-files.ps1` - Generate project files
- `run-all-services.ps1` - Run all services

### Development Workflow
1. Create feature branch
2. Make changes
3. Test locally
4. Submit pull request

---

## ?? Support

### Need Help?
1. Check [QUICKSTART.md](QUICKSTART.md)
2. Review [USER_GUIDE.md](USER_GUIDE.md)
3. See [Troubleshooting](#troubleshooting)
4. Check service logs

### Troubleshooting
- **Port in use**: Change port in run command
- **Database error**: Check SQL Server is running
- **Auth error**: Verify JWT token is valid
- **Build error**: Run `dotnet clean` then `dotnet build`

---

## ?? Roadmap

### v2.0 (Current) ?
- [x] Microservices architecture
- [x] 8 independent services
- [x] Complete documentation
- [x] Docker support

### v2.1 (Next)
- [ ] API Gateway (Ocelot/YARP)
- [ ] Service discovery
- [ ] Distributed caching (Redis)
- [ ] Message queue (RabbitMQ)

### v3.0 (Future)
- [ ] Kubernetes deployment
- [ ] Distributed tracing
- [ ] CI/CD pipelines
- [ ] Database per service

---

## ?? License

Copyright © 2024 WMS Team

---

## ?? Team

**Architecture**: Microservices  
**Technology**: .NET 9.0  
**Database**: SQL Server  
**Version**: 2.0.0

---

<div align="center">

**? Star this project if you find it useful!**

[Documentation](MICROSERVICES_ARCHITECTURE.md) • [Quick Start](QUICKSTART.md) • [User Guide](USER_GUIDE.md)

</div>
