using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WMS.Domain.Middleware;

namespace WMS.Domain.Extensions;

/// <summary>
/// Extension methods for configuring global exception handling in API projects
/// </summary>
public static class ExceptionHandlerExtensions
{
    /// <summary>
    /// Adds global exception handler to the service collection
    /// Call this in Program.cs: builder.Services.AddGlobalExceptionHandler();
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(); // Adds support for RFC 7807 Problem Details
        
        return services;
    }

    /// <summary>
    /// Configures the exception handler middleware in the request pipeline
    /// Call this in Program.cs: app.UseGlobalExceptionHandler();
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(options => { }); // Empty options uses the registered IExceptionHandler
        
        return app;
    }
}
