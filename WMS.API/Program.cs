using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using WMS.Application.Interfaces;
using WMS.Domain.Interfaces;
//using WMS.Infrastructure.Data;
//using WMS.Infrastructure.Repositories;
//using WMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<WMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WMS.Infrastructure")));

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
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

// Dependency Injection - Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInboundService, InboundService>();
builder.Services.AddScoped<IOutboundService, OutboundService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService>(sp => new TokenService(
    secretKey,
    jwtSettings["Issuer"] ?? "WMS.API",
    jwtSettings["Audience"] ?? "WMS.Client",
    int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")
));

var app = builder.Build();

// Diagnostic: enumerate assemblies and surface ReflectionTypeLoadException details
try
{
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            // Force type resolution to reveal loader exceptions for that assembly
            _ = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException rtlex)
        {
            Console.WriteLine($"Failed to load types from assembly: {asm.FullName}");
            Console.WriteLine(rtlex.Message);
            if (rtlex.LoaderExceptions != null)
            {
                foreach (var le in rtlex.LoaderExceptions)
                {
                    Console.WriteLine("LoaderException: " + (le?.Message ?? "<null>"));
                }
            }
        }
        catch (Exception ex)
        {
            // Non-reflection errors when getting types
            Console.WriteLine($"Error getting types from assembly {asm.FullName}: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Assembly inspection failure: " + ex.Message);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WMS API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migrate database on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WMSDbContext>();
    // dbContext.Database.Migrate(); // Uncomment after creating migrations
}

// Wrap Run to capture any ReflectionTypeLoadException at startup
try
{
    app.Run();
}
catch (ReflectionTypeLoadException rtlex)
{
    Console.WriteLine("ReflectionTypeLoadException during app.Run(): " + rtlex.Message);
    if (rtlex.LoaderExceptions != null)
    {
        foreach (var le in rtlex.LoaderExceptions)
        {
            Console.WriteLine("LoaderException: " + (le?.Message ?? "<null>"));
        }
    }
    throw;
}
catch (Exception ex)
{
    Console.WriteLine("Unhandled exception during app.Run(): " + ex.Message);
    throw;
}
