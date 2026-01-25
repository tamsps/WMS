using MediatR;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.ProcessDeliveryWebhook;

/// <summary>
/// Command to process delivery partner webhook
/// Implements idempotency to handle duplicate webhooks safely
/// </summary>
public class ProcessDeliveryWebhookCommand : IRequest<Result>
{
    public DeliveryWebhookDto Dto { get; set; } = null!;
}
