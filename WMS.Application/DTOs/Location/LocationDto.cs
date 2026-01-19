namespace WMS.Application.DTOs.Location;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public string Rack { get; set; } = string.Empty;
    public string Shelf { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public decimal CurrentOccupancy { get; set; }
    public decimal AvailableCapacity => Capacity - CurrentOccupancy;
    public bool IsActive { get; set; }
    public string? LocationType { get; set; }
    public Guid? ParentLocationId { get; set; }
}

public class CreateLocationDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Aisle { get; set; } = string.Empty;
    public string Rack { get; set; } = string.Empty;
    public string Shelf { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public string? LocationType { get; set; }
    public Guid? ParentLocationId { get; set; }
}

public class UpdateLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public bool IsActive { get; set; }
}
