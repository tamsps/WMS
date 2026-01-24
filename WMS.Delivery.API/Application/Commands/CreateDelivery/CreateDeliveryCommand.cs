using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.CreateDelivery;

public class CreateDeliveryCommand : IRequest<Result<DeliveryDto>>
{
    public CreateDeliveryDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
