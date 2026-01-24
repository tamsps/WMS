using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Queries.GetDeliveryById;

public class GetDeliveryByIdQuery : IRequest<Result<DeliveryDto>>
{
    public Guid Id { get; set; }
}
