using WMS.Domain.Common;

namespace WMS.Domain.Entities;

/// <summary>
/// Warehouse Location Entity - Physical storage position
/// Represents hierarchical warehouse structure with capacity enforcement
/// 
/// Location Hierarchy Management:
/// - Locations can be organized in a hierarchical structure using ParentLocationId
/// - Each location has a defined capacity that must be enforced
/// - CurrentOccupancy tracks the amount of space currently used
/// - Available capacity = Capacity - CurrentOccupancy
/// - Only active locations can be used in warehouse transactions
/// - Location Code is unique and immutable once created
/// 
/// Capacity Enforcement:
/// - When inventory is added to a location, CurrentOccupancy must be updated
/// - Inbound operations must check available capacity before receiving goods
/// - Capacity checks should consider product dimensions and quantities
/// </summary>
public class Location : BaseEntity
{
    /// <summary>
    /// Unique location code - immutable identifier
    /// Cannot be changed after location creation
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Location display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Location description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Warehouse zone identifier
    /// </summary>
    public string Zone { get; set; } = string.Empty;
    
    /// <summary>
    /// Aisle identifier within the zone
    /// </summary>
    public string Aisle { get; set; } = string.Empty;
    
    /// <summary>
    /// Rack identifier within the aisle
    /// </summary>
    public string Rack { get; set; } = string.Empty;
    
    /// <summary>
    /// Shelf identifier within the rack
    /// </summary>
    public string Shelf { get; set; } = string.Empty;
    
    /// <summary>
    /// Bin identifier within the shelf
    /// </summary>
    public string Bin { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum capacity of the location (in cubic meters or defined unit)
    /// This is the total storage space available
    /// </summary>
    public decimal Capacity { get; set; }
    
    /// <summary>
    /// Current occupancy of the location (in cubic meters or defined unit)
    /// Must be updated when inventory is added or removed
    /// Must never exceed Capacity
    /// </summary>
    public decimal CurrentOccupancy { get; set; }
    
    /// <summary>
    /// Indicates if the location is active
    /// Only active locations can be used in transactions
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Type of location (e.g., Receiving, Storage, Picking, Shipping)
    /// </summary>
    public string? LocationType { get; set; }
    
    // Hierarchical structure
    
    /// <summary>
    /// Parent location ID for hierarchical structure
    /// Null if this is a top-level location
    /// </summary>
    public Guid? ParentLocationId { get; set; }
    
    /// <summary>
    /// Parent location navigation property
    /// </summary>
    public virtual Location? ParentLocation { get; set; }
    
    /// <summary>
    /// Child locations in the hierarchy
    /// </summary>
    public virtual ICollection<Location> ChildLocations { get; set; } = new List<Location>();
    
    // Navigation properties
    
    /// <summary>
    /// Current inventory stored at this location
    /// </summary>
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    
    /// <summary>
    /// Inbound items designated for this location
    /// </summary>
    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();
    
    /// <summary>
    /// Outbound items picked from this location
    /// </summary>
    public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();
    
    /// <summary>
    /// Calculate available capacity
    /// </summary>
    public decimal GetAvailableCapacity() => Capacity - CurrentOccupancy;
    
    /// <summary>
    /// Check if the location has sufficient capacity for the given amount
    /// </summary>
    public bool HasCapacityFor(decimal requiredCapacity) => GetAvailableCapacity() >= requiredCapacity;
}
