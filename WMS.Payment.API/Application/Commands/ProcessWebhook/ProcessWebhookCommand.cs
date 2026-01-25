using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.ProcessWebhook;

/// <summary>
/// Command to process payment gateway webhook
/// Implements idempotency to handle duplicate webhooks safely
/// </summary>
public class ProcessWebhookCommand : IRequest<Result>
{
    public PaymentWebhookDto Dto { get; set; } = null!;
}
