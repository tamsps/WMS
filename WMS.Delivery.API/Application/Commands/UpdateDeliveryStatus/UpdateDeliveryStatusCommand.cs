using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.UpdateDeliveryStatus;

public class UpdateDeliveryStatusCommand : IRequest<Result<DeliveryDto>>
{
    public UpdateDeliveryStatusDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
