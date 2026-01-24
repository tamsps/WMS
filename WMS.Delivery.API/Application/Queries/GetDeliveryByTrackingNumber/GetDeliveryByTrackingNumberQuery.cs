using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Queries.GetDeliveryByTrackingNumber;

public class GetDeliveryByTrackingNumberQuery : IRequest<Result<DeliveryDto>>
{
    public string TrackingNumber { get; set; } = null!;
}
