namespace WMS.Locations.API.DTOs.Location;

/// <summary>
/// Data transfer object for location information
/// </summary>
public class LocationDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Unique location code - immutable identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public string Rack { get; set; } = string.Empty;
    public string Shelf { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum capacity in cubic meters
    /// </summary>
    public decimal Capacity { get; set; }
    
    /// <summary>
    /// Current occupancy in cubic meters
    /// </summary>
    public decimal CurrentOccupancy { get; set; }
    
    /// <summary>
    /// Available capacity (calculated property)
    /// </summary>
    public decimal AvailableCapacity => Capacity - CurrentOccupancy;
    
    /// <summary>
    /// Indicates if the location is active
    /// Only active locations can be used in transactions
    /// </summary>
    public bool IsActive { get; set; }
    
    public string? LocationType { get; set; }
    
    /// <summary>
    /// Parent location ID for hierarchical structure
    /// </summary>
    public Guid? ParentLocationId { get; set; }
}

/// <summary>
/// Data transfer object for creating a new location
/// </summary>
public class CreateLocationDto
{
    /// <summary>
    /// Unique location code - must be unique across all locations
    /// Once set, it cannot be changed
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public string Rack { get; set; } = string.Empty;
    public string Shelf { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum capacity in cubic meters
    /// Must be greater than 0
    /// </summary>
    public decimal Capacity { get; set; }
    
    public string? LocationType { get; set; }
    public Guid? ParentLocationId { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing location
/// Note: Code is not included as it is immutable and cannot be changed
/// Note: CurrentOccupancy is not included as it's managed by the system
/// </summary>
public class UpdateLocationDto
{
    public Guid Id { get; set; }
    
    // Code is intentionally excluded - it is immutable and cannot be updated
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum capacity in cubic meters
    /// Cannot be reduced below current occupancy
    /// </summary>
    public decimal Capacity { get; set; }
    
    public bool IsActive { get; set; }
}
