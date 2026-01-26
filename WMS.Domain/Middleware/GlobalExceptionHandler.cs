using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace WMS.Domain.Middleware;

/// <summary>
/// Global exception handler for all API projects
/// Catches all unhandled exceptions, logs them, and returns standardized error responses
/// Eliminates the need for try-catch blocks in controllers and handlers
/// 
/// Usage in Program.cs:
/// builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
/// app.UseExceptionHandler(options => { });
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception with full details
        _logger.LogError(
            exception,
            "Unhandled exception: {ExceptionType} | Message: {Message} | Path: {Path} | Method: {Method} | TraceId: {TraceId}",
            exception.GetType().Name,
            exception.Message,
            httpContext.Request.Path,
            httpContext.Request.Method,
            httpContext.TraceIdentifier);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception has been handled
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var title = GetTitle(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = GetDetailMessage(exception),
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["exceptionType"] = exception.GetType().Name
            }
        };

        // Add validation errors if present
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        // Custom exceptions
        BusinessRuleViolationException => (int)HttpStatusCode.BadRequest,
        ResourceNotFoundException => (int)HttpStatusCode.NotFound,
        ValidationException => (int)HttpStatusCode.BadRequest,
        ConcurrencyException => (int)HttpStatusCode.Conflict,
        
        // EF Core exceptions
        DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
        DbUpdateException => (int)HttpStatusCode.BadRequest,
        
        // Standard exceptions
        ArgumentNullException => (int)HttpStatusCode.BadRequest,
        ArgumentException => (int)HttpStatusCode.BadRequest,
        InvalidOperationException => (int)HttpStatusCode.BadRequest,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
        NotImplementedException => (int)HttpStatusCode.NotImplemented,
        TimeoutException => (int)HttpStatusCode.RequestTimeout,
        
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        BusinessRuleViolationException => "Business Rule Violation",
        ResourceNotFoundException => "Resource Not Found",
        ValidationException => "Validation Failed",
        ConcurrencyException => "Concurrency Conflict",
        DbUpdateConcurrencyException => "Concurrency Conflict",
        DbUpdateException => "Database Update Failed",
        ArgumentNullException => "Bad Request - Null Argument",
        ArgumentException => "Bad Request - Invalid Argument",
        InvalidOperationException => "Bad Request - Invalid Operation",
        KeyNotFoundException => "Resource Not Found",
        UnauthorizedAccessException => "Unauthorized Access",
        NotImplementedException => "Feature Not Implemented",
        TimeoutException => "Request Timeout",
        _ => "Internal Server Error"
    };

    private static string GetDetailMessage(Exception exception)
    {
        return exception switch
        {
            DbUpdateConcurrencyException => 
                "The resource was modified by another user. Please refresh and try again.",
            DbUpdateException dbEx when dbEx.InnerException != null => 
                $"{exception.Message} | {dbEx.InnerException.Message}",
            _ => exception.Message
        };
    }
}

/// <summary>
/// Custom exception for business rule violations
/// Returns HTTP 400 Bad Request
/// 
/// Example: throw new BusinessRuleViolationException("Cannot ship prepaid order without payment confirmation");
/// </summary>
public class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Custom exception for resource not found scenarios
/// Returns HTTP 404 Not Found
/// 
/// Example: throw new ResourceNotFoundException("Product", productId);
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found.")
    {
    }

    public ResourceNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Custom exception for validation errors
/// Returns HTTP 400 Bad Request
/// 
/// Example: throw new ValidationException("SKU", "SKU already exists");
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base($"Validation failed for '{propertyName}': {errorMessage}")
    {
        Errors = new Dictionary<string, string[]>
        {
            [propertyName] = new[] { errorMessage }
        };
    }
}

/// <summary>
/// Custom exception for concurrency conflicts (optimistic locking)
/// Returns HTTP 409 Conflict
/// 
/// Example: throw new ConcurrencyException("Inventory was modified during shipping");
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException()
        : base("The resource was modified by another user. Please refresh and try again.")
    {
    }
}
