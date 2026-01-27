namespace WMS.Outbound.API.DTOs.Outbound;

public class OutboundDto
{
    public Guid Id { get; set; }
    public string OutboundNumber { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? PaymentId { get; set; }
    public string? PaymentStatus { get; set; }
    public List<OutboundItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OutboundItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }
    /// <summary>
    /// Available quantity from inventory (QuantityOnHand - QuantityReserved)
    /// Used by Pick UI to show how much can be picked
    /// </summary>
    public decimal AvailableQuantity { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
    public string UOM { get; set; } = string.Empty;
}

public class CreateOutboundDto
{
    public string? OrderNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateOutboundItemDto> Items { get; set; } = new();
}

public class CreateOutboundItemDto
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal OrderedQuantity { get; set; }
}

public class PickOutboundDto
{
    public Guid OutboundId { get; set; }
    public List<PickOutboundItemDto> Items { get; set; } = new();
}

public class PickOutboundItemDto
{
    public Guid OutboundItemId { get; set; }
    public decimal PickedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class ShipOutboundDto
{
    public Guid OutboundId { get; set; }
}
