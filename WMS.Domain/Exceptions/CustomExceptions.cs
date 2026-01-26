namespace WMS.Domain.Exceptions;

/// <summary>
/// Custom exception for business rule violations
/// Returns HTTP 400 Bad Request when caught by global exception handler
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
/// Returns HTTP 404 Not Found when caught by global exception handler
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
/// Returns HTTP 400 Bad Request when caught by global exception handler
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
/// Returns HTTP 409 Conflict when caught by global exception handler
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
