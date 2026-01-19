using WMS.Domain.Common;

namespace WMS.Domain.Entities;

/// <summary>
/// Warehouse Location Entity - Physical storage position
/// Represents hierarchical warehouse structure with capacity enforcement
/// </summary>
public class Location : BaseEntity
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
    public decimal CurrentOccupancy { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LocationType { get; set; } // Receiving, Storage, Picking, Shipping
    
    // Hierarchical structure
    public Guid? ParentLocationId { get; set; }
    public virtual Location? ParentLocation { get; set; }
    public virtual ICollection<Location> ChildLocations { get; set; } = new List<Location>();
    
    // Navigation properties
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();
    public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();
}
