namespace WMS.Inbound.API.DTOs.Inbound;

/// <summary>
/// Data transfer object for inbound information
/// </summary>
public class InboundDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Unique inbound number (auto-generated)
    /// </summary>
    public string InboundNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// External reference number (e.g., PO Number)
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Current status: Pending, Received, PutAway, Completed, Cancelled
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    public DateTime ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Line items in this inbound shipment
    /// </summary>
    public List<InboundItemDto> Items { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for inbound item information
/// </summary>
public class InboundItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    
    /// <summary>
    /// Expected quantity from purchase order
    /// </summary>
    public decimal ExpectedQuantity { get; set; }
    
    /// <summary>
    /// Actual quantity received (including damaged)
    /// </summary>
    public decimal ReceivedQuantity { get; set; }
    
    /// <summary>
    /// Quantity of damaged items (not added to inventory)
    /// </summary>
    public decimal? DamagedQuantity { get; set; }
    
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for creating a new inbound shipment
/// </summary>
public class CreateInboundDto
{
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Expected delivery date
    /// Must not be in the past
    /// </summary>
    public DateTime ExpectedDate { get; set; }
    
    /// <summary>
    /// Supplier name (required)
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;
    
    public string? SupplierCode { get; set; }
    public string? Notes { get; set; }
    
    /// <summary>
    /// Line items (at least one required)
    /// </summary>
    public List<CreateInboundItemDto> Items { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating an inbound item
/// </summary>
public class CreateInboundItemDto
{
    /// <summary>
    /// Product ID (must be an active product)
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Destination location (must be an active location with sufficient capacity)
    /// </summary>
    public Guid LocationId { get; set; }
    
    /// <summary>
    /// Expected quantity (must be greater than 0)
    /// </summary>
    public decimal ExpectedQuantity { get; set; }
    
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for receiving inbound items
/// Atomic transaction: All items must be successfully received or transaction is rolled back
/// </summary>
public class ReceiveInboundDto
{
    public Guid InboundId { get; set; }
    
    /// <summary>
    /// Items to receive (at least one required)
    /// </summary>
    public List<ReceiveInboundItemDto> Items { get; set; } = new();
}

/// <summary>
/// Data transfer object for receiving an individual inbound item
/// </summary>
public class ReceiveInboundItemDto
{
    public Guid InboundItemId { get; set; }
    
    /// <summary>
    /// Actual quantity received (including damaged)
    /// Must be >= 0
    /// </summary>
    public decimal ReceivedQuantity { get; set; }
    
    /// <summary>
    /// Quantity of damaged items (not added to inventory)
    /// Must be >= 0 and <= ReceivedQuantity
    /// </summary>
    public decimal? DamagedQuantity { get; set; }
    
    /// <summary>
    /// Notes about receiving this item (e.g., quality issues)
    /// </summary>
    public string? Notes { get; set; }
}
