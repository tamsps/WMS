using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.ProcessDeliveryWebhook;

public class ProcessDeliveryWebhookCommandValidator : AbstractValidator<ProcessDeliveryWebhookCommand>
{
    public ProcessDeliveryWebhookCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Webhook data is required");

        RuleFor(x => x.Dto.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required")
            .MaximumLength(100).WithMessage("TrackingNumber cannot exceed 100 characters");

        RuleFor(x => x.Dto.PartnerEventId)
            .NotEmpty().WithMessage("PartnerEventId is required for idempotency")
            .MaximumLength(200).WithMessage("PartnerEventId cannot exceed 200 characters");

        RuleFor(x => x.Dto.Status)
            .NotEmpty().WithMessage("Delivery status is required");

        RuleFor(x => x.Dto.EventData)
            .NotEmpty().WithMessage("Event data is required for audit");
    }
}
