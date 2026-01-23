# Generate all microservice project files and Program.cs

$services = @(
    @{Name="Products"; Port=5002; Services=@("IProductService")},
    @{Name="Locations"; Port=5003; Services=@("ILocationService")},
    @{Name="Inventory"; Port=5004; Services=@("IInventoryService")},
    @{Name="Inbound"; Port=5005; Services=@("IInboundService", "IInventoryService")},
    @{Name="Outbound"; Port=5006; Services=@("IOutboundService", "IInventoryService")},
    @{Name="Payment"; Port=5007; Services=@("IPaymentService")},
    @{Name="Delivery"; Port=5008; Services=@("IDeliveryService")}
)

foreach ($svc in $services) {
    $projectDir = "WMS.$($svc.Name).API"
    $projectFile = "$projectDir\WMS.$($svc.Name).API.csproj"
    
    # Create .csproj
    $csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.7" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="7.0.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\WMS.Application\WMS.Application.csproj" />
    <ProjectReference Include="..\WMS.Infrastructure\WMS.Infrastructure.csproj" />
    <ProjectReference Include="..\WMS.Domain\WMS.Domain.csproj" />
  </ItemGroup>

</Project>
"@
    Set-Content -Path $projectFile -Value $csprojContent
    
    # Create Program.cs
    $serviceRegistrations = ($svc.Services | ForEach-Object { 
        $interface = $_
        $impl = $_ -replace "^I", ""
        "builder.Services.AddScoped<$interface, $impl>();"
    }) -join "`n"
    
    $programContent = @"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WMS.Application.Interfaces;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;
using WMS.Infrastructure.Repositories;
using WMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WMS $($svc.Name) API", Version = "v1" });
});

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
$serviceRegistrations

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WMS $($svc.Name) API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
"@
    Set-Content -Path "$projectDir\Program.cs" -Value $programContent
    
    # Create appsettings.json
    $appsettingsContent = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WMSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourVeryLongSecretKeyForJWTTokenGeneration_MinimumLength32Characters",
    "Issuer": "WMS.$($svc.Name).API",
    "Audience": "WMS.Client",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:5000", "https://localhost:5000" ]
  }
}
"@
    Set-Content -Path "$projectDir\appsettings.json" -Value $appsettingsContent
    
    Write-Host "? Generated project files for WMS.$($svc.Name).API" -ForegroundColor Green
}

Write-Host ""
Write-Host "All microservice project files generated successfully!" -ForegroundColor Green
