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

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    gateway = "WMS API Gateway",
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithOpenApi();

// Gateway info endpoint
app.MapGet("/gateway/info", () => Results.Ok(new
{
    name = "WMS API Gateway",
    version = "1.0.0",
    description = "Unified entry point for all WMS microservices",
    services = new[]
    {
        new { name = "Auth", route = "/auth", port = 5001 },
        new { name = "Products", route = "/products", port = 5002 },
        new { name = "Locations", route = "/locations", port = 5003 },
        new { name = "Inventory", route = "/inventory", port = 5006 },
        new { name = "Inbound", route = "/inbound", port = 5004 },
        new { name = "Outbound", route = "/outbound", port = 5005 },
        new { name = "Payment", route = "/payment", port = 5007 },
        new { name = "Delivery", route = "/delivery", port = 5008 }
    }
}))
.WithName("GatewayInfo")
.WithOpenApi();

app.Run();
