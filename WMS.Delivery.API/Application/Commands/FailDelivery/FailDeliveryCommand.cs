using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.FailDelivery;

public class FailDeliveryCommand : IRequest<Result<DeliveryDto>>
{
    public FailDeliveryDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
