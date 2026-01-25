using FluentValidation;

namespace WMS.Payment.API.Application.Commands.ProcessWebhook;

public class ProcessWebhookCommandValidator : AbstractValidator<ProcessWebhookCommand>
{
    public ProcessWebhookCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Webhook data is required");

        RuleFor(x => x.Dto.ExternalPaymentId)
            .NotEmpty().WithMessage("ExternalPaymentId is required")
            .MaximumLength(100).WithMessage("ExternalPaymentId cannot exceed 100 characters");

        RuleFor(x => x.Dto.GatewayEventId)
            .NotEmpty().WithMessage("GatewayEventId is required for idempotency")
            .MaximumLength(200).WithMessage("GatewayEventId cannot exceed 200 characters");

        RuleFor(x => x.Dto.Status)
            .NotEmpty().WithMessage("Payment status is required");

        RuleFor(x => x.Dto.EventData)
            .NotEmpty().WithMessage("Event data is required for audit");
    }
}
