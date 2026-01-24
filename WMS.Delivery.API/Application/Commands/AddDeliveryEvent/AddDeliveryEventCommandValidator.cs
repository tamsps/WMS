using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.AddDeliveryEvent;

public class AddDeliveryEventCommandValidator : AbstractValidator<AddDeliveryEventCommand>
{
    public AddDeliveryEventCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Event data is required");

        RuleFor(x => x.Dto.DeliveryId)
            .NotEmpty().WithMessage("Delivery ID is required");

        RuleFor(x => x.Dto.EventType)
            .NotEmpty().WithMessage("Event type is required");

        RuleFor(x => x.Dto.EventDescription)
            .NotEmpty().WithMessage("Event description is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
