namespace WMS.Inbound.API.DTOs.Inbound;

public class InboundDto
{
    public Guid Id { get; set; }
    public string InboundNumber { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? Notes { get; set; }
    public List<InboundItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class InboundItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public decimal ExpectedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? DamagedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateInboundDto
{
    public string? ReferenceNumber { get; set; }
    public DateTime ExpectedDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? Notes { get; set; }
    public List<CreateInboundItemDto> Items { get; set; } = new();
}

public class CreateInboundItemDto
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal ExpectedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class ReceiveInboundDto
{
    public Guid InboundId { get; set; }
    public List<ReceiveInboundItemDto> Items { get; set; } = new();
}

public class ReceiveInboundItemDto
{
    public Guid InboundItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? DamagedQuantity { get; set; }
    public string? Notes { get; set; }
}
