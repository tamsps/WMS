namespace WMS.Application.DTOs.Delivery;

public class DeliveryDto
{
    public Guid Id { get; set; }
    public string DeliveryNumber { get; set; } = string.Empty;
    public Guid OutboundId { get; set; }
    public string OutboundNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? FailureReason { get; set; }
    public bool IsReturn { get; set; }
    public List<DeliveryEventDto> Events { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class DeliveryEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class CreateDeliveryDto
{
    public Guid OutboundId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EventLocation { get; set; }
    public string? Notes { get; set; }
}

public class CompleteDeliveryDto
{
    public Guid DeliveryId { get; set; }
    public string? Notes { get; set; }
}

public class FailDeliveryDto
{
    public Guid DeliveryId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
}

public class AddDeliveryEventDto
{
    public Guid DeliveryId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Notes { get; set; }
}
