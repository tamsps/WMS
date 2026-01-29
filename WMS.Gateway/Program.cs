var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger for Gateway documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "WMS API Gateway",
        Version = "v1",
        Description = "Unified API Gateway for all WMS Microservices"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WMS API Gateway V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map YARP reverse proxy
app.MapReverseProxy();

// Health check endpoint - Removed .WithOpenApi() to avoid version conflict
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    gateway = "WMS API Gateway",
    version = "1.0.0"
}))
.WithName("HealthCheck");

// Gateway info endpoint - Removed .WithOpenApi() to avoid version conflict
app.MapGet("/gateway/info", () => Results.Ok(new
{
    name = "WMS API Gateway",
    version = "1.0.0",
    description = "Unified entry point for all WMS microservices",
    gatewayUrl = "http://localhost:5000",
    services = new[]
    {
        new { 
            name = "Auth", 
            route = "/auth", 
            backendUrl = "http://localhost:5190",
            endpoints = new[] { 
                "/auth/login", 
                "/auth/register", 
                "/auth/refresh", 
                "/auth/me", 
                "/auth/validate" 
            }
        },
        new { 
            name = "Products", 
            route = "/products", 
            backendUrl = "https://localhost:62527",
            endpoints = new[] { 
                "/products", 
                "/products/{id}", 
                "/products/sku/{sku}",
                "/products/{id}/activate",
                "/products/{id}/deactivate"
            }
        },
        new { 
            name = "Locations", 
            route = "/locations", 
            backendUrl = "https://localhost:62522",
            endpoints = new[] { 
                "/locations", 
                "/locations/{id}", 
                "/locations/code/{code}",
                "/locations/{id}/activate",
                "/locations/{id}/deactivate"
            }
        },
        new { 
            name = "Inventory", 
            route = "/inventory", 
            backendUrl = "https://localhost:62531",
            endpoints = new[] { 
                "/inventory", 
                "/inventory/{id}", 
                "/inventory/product/{productId}",
                "/inventory/location/{locationId}",
                "/inventory/transactions",
                "/inventory/adjust",
                "/inventory/transfer"
            }
        },
        new { 
            name = "Inbound", 
            route = "/inbound", 
            backendUrl = "https://localhost:62520",
            endpoints = new[] { 
                "/inbound", 
                "/inbound/{id}", 
                "/inbound/receive",
                "/inbound/{id}/cancel"
            }
        },
        new { 
            name = "Outbound", 
            route = "/outbound", 
            backendUrl = "https://localhost:62519",
            endpoints = new[] { 
                "/outbound", 
                "/outbound/{id}", 
                "/outbound/pick",
                "/outbound/ship",
                "/outbound/{id}/cancel"
            }
        },
        new { 
            name = "Payment", 
            route = "/payment", 
            backendUrl = "https://localhost:62521",
            endpoints = new[] { 
                "/payment", 
                "/payment/{id}", 
                "/payment/confirm",
                "/payment/{id}/cancel"
            }
        },
        new { 
            name = "Delivery", 
            route = "/delivery", 
            backendUrl = "https://localhost:62529",
            endpoints = new[] { 
                "/delivery", 
                "/delivery/{id}", 
                "/delivery/tracking/{trackingNumber}",
                "/delivery/status",
                "/delivery/complete",
                "/delivery/fail",
                "/delivery/event"
            }
        }
    }
}))
.WithName("GatewayInfo");

app.Run();
