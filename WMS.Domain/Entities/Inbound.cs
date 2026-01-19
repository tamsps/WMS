using WMS.Domain.Common;
using WMS.Domain.Enums;

namespace WMS.Domain.Entities;

/// <summary>
/// Inbound Entity - Receiving goods process
/// Increases inventory through atomic transactions
/// </summary>
public class Inbound : BaseEntity
{
    public string InboundNumber { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; } // PO Number, Transfer Order, etc.
    public InboundStatus Status { get; set; } = InboundStatus.Pending;
    public DateTime ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();
}

public class InboundItem : BaseEntity
{
    public Guid InboundId { get; set; }
    public virtual Inbound Inbound { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public decimal ExpectedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? DamagedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}
