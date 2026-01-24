# ?? Build Error Fix - Program.cs Files

## Issue
During the automated refactoring, several Program.cs files were truncated causing build errors.

## Affected Files
- WMS.Inbound.API\Program.cs
- WMS.Outbound.API\Program.cs
- WMS.Inventory.API\Program.cs
- WMS.Locations.API\Program.cs
- WMS.Products.API\Program.cs
- WMS.Payment.API\Program.cs
- WMS.Delivery.API\Program.cs
- WMS.Auth.API\Program.cs

## Root Cause
String truncation in GetSection("JwtSettings") - missing closing quote and semicolon.

## Fix Required

Each Program.cs file needs to have this pattern:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WMS.[ServiceName].API.Interfaces;  // ? Change per microservice
using WMS.Domain.Interfaces;
using WMS.Domain.Data;                    // ? Changed from WMS.Infrastructure.Data
using WMS.Domain.Repositories;             // ? Changed from WMS.Infrastructure.Repositories
using WMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WMS [Service] API", Version = "v1" });
});

// Database Configuration
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Domain")));  // ? Changed from "WMS.Infrastructure"

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");  // ? FIX: Add closing quote and semicolon
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Dependency Injection - Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Dependency Injection - Services (temporary - will be replaced by CQRS)
builder.Services.AddScoped<I[ServiceName]Service, [ServiceName]Service>();  // ? Change per microservice

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WMS [Service] API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Key Changes Made:
1. ? Changed `using WMS.Infrastructure.Data` ? `using WMS.Domain.Data`
2. ? Changed `using WMS.Infrastructure.Repositories` ? `using WMS.Domain.Repositories`
3. ? Changed `b.MigrationsAssembly("WMS.Infrastructure")` ? `b.MigrationsAssembly("WMS.Domain")`
4. ? INCOMPLETE: String truncation error on line 28

## Immediate Action Required

I need to provide complete Program.cs files for all microservices. 

**Should I:**
1. Continue with automated fix (provide complete files for all 8 microservices)?
2. Provide a rollback script to revert to previous state?
3. Create a manual fix guide for you to implement?

**Recommendation:** Option 1 - Let me provide complete, tested Program.cs files for all microservices.

Awaiting your direction...
