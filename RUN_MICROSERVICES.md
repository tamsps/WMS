# Running WMS Microservices

## Quick Start Guide

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 / VS Code / Rider

### Option 1: Run All Services Individually

Open 8 separate terminal windows and run each service:

```powershell
# Terminal 1 - Auth Service
cd WMS.Auth.API
dotnet run --urls=https://localhost:5001

# Terminal 2 - Products Service
cd WMS.Products.API
dotnet run --urls=https://localhost:5002

# Terminal 3 - Locations Service
cd WMS.Locations.API
dotnet run --urls=https://localhost:5003

# Terminal 4 - Inventory Service
cd WMS.Inventory.API
dotnet run --urls=https://localhost:5004

# Terminal 5 - Inbound Service
cd WMS.Inbound.API
dotnet run --urls=https://localhost:5005

# Terminal 6 - Outbound Service
cd WMS.Outbound.API
dotnet run --urls=https://localhost:5006

# Terminal 7 - Payment Service
cd WMS.Payment.API
dotnet run --urls=https://localhost:5007

# Terminal 8 - Delivery Service
cd WMS.Delivery.API
dotnet run --urls=https://localhost:5008

# Terminal 9 - Web Application
cd WMS.Web
dotnet run --urls=https://localhost:5000
```

### Option 2: Use PowerShell Script (Recommended for Development)

Run the included PowerShell script to start all services:

```powershell
.\run-all-services.ps1
```

### Option 3: Docker Compose (Recommended for Production)

```bash
docker-compose up
```

## Service URLs

| Service | URL | Swagger UI |
|---------|-----|------------|
| Auth API | https://localhost:5001 | https://localhost:5001 |
| Products API | https://localhost:5002 | https://localhost:5002 |
| Locations API | https://localhost:5003 | https://localhost:5003 |
| Inventory API | https://localhost:5004 | https://localhost:5004 |
| Inbound API | https://localhost:5005 | https://localhost:5005 |
| Outbound API | https://localhost:5006 | https://localhost:5006 |
| Payment API | https://localhost:5007 | https://localhost:5007 |
| Delivery API | https://localhost:5008 | https://localhost:5008 |
| Web Application | https://localhost:5000 | N/A |

## Testing the Microservices

### 1. Test Authentication
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### 2. Test Products API (with token)
```bash
curl -X GET https://localhost:5002/api/products \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Access Swagger UI
Navigate to any service URL in your browser to access Swagger documentation.

## Configuration

Each microservice has its own `appsettings.json`:
- Database connection string
- JWT settings (must match across all services)
- CORS settings
- Service-specific settings

## Troubleshooting

### Port Already in Use
If a port is already in use, update the port in:
1. `appsettings.json` ? Service URLs
2. `docker-compose.yml` ? Port mappings
3. Run command ? `--urls` parameter

### Database Connection Issues
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Run migrations: `dotnet ef database update --project WMS.Infrastructure --startup-project WMS.Auth.API`

### JWT Token Issues
- Ensure all services use the same `JwtSettings:SecretKey`
- Check token expiration settings
- Verify Issuer and Audience match

## Development Tips

### Watch Mode
Run any service with hot reload:
```bash
dotnet watch run --project WMS.Auth.API --urls=https://localhost:5001
```

### Debugging Multiple Services in Visual Studio
1. Right-click Solution ? Properties
2. Select "Multiple startup projects"
3. Set all WMS.*.API projects to "Start"
4. Click OK and press F5

### Viewing Logs
All services log to console. For production, configure:
- Seq
- ELK Stack
- Application Insights
- Serilog

## Next Steps

1. ? All microservices running
2. ? Authentication working
3. ? Implement API Gateway (Ocelot/YARP)
4. ? Add Redis caching
5. ? Implement message queue (RabbitMQ)
6. ? Add distributed tracing
7. ? Set up CI/CD pipelines
8. ? Deploy to Kubernetes

## Architecture Benefits

? **Independent Deployment** - Deploy one service without affecting others  
? **Technology Flexibility** - Use different technologies per service  
? **Scalability** - Scale high-traffic services independently  
? **Fault Isolation** - One service failure doesn't crash the system  
? **Team Autonomy** - Different teams can own different services  

---

**Last Updated:** January 2024  
**Version:** 1.0.0
