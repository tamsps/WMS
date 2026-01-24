using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.CompleteDelivery;

public class CompleteDeliveryCommand : IRequest<Result<DeliveryDto>>
{
    public CompleteDeliveryDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
