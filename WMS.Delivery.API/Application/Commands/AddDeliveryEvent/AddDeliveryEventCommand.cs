using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.AddDeliveryEvent;

public class AddDeliveryEventCommand : IRequest<Result<DeliveryDto>>
{
    public AddDeliveryEventDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
