using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Mappers;

public static class DeliveryMapper
{
    public static DeliveryDto MapToDto(WMS.Domain.Entities.Delivery delivery)
    {
        return new DeliveryDto
        {
            Id = delivery.Id,
            DeliveryNumber = delivery.DeliveryNumber,
            OutboundId = delivery.OutboundId,
            OutboundNumber = delivery.Outbound?.OutboundNumber ?? string.Empty,
            Status = delivery.Status.ToString(),
            ShippingAddress = delivery.ShippingAddress,
            ShippingCity = delivery.ShippingCity,
            ShippingState = delivery.ShippingState,
            ShippingZipCode = delivery.ShippingZipCode,
            ShippingCountry = delivery.ShippingCountry,
            Carrier = delivery.Carrier,
            TrackingNumber = delivery.TrackingNumber,
            VehicleNumber = delivery.VehicleNumber,
            DriverName = delivery.DriverName,
            DriverPhone = delivery.DriverPhone,
            PickupDate = delivery.PickupDate,
            EstimatedDeliveryDate = delivery.EstimatedDeliveryDate,
            ActualDeliveryDate = delivery.ActualDeliveryDate,
            DeliveryNotes = delivery.DeliveryNotes,
            FailureReason = delivery.FailureReason,
            IsReturn = delivery.IsReturn,
            Events = delivery.DeliveryEvents.Select(e => new DeliveryEventDto
            {
                Id = e.Id,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Notes = e.Notes
            }).ToList(),
            CreatedAt = delivery.CreatedAt
        };
    }
}
