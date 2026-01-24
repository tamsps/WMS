using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.CompleteDelivery;

public class CompleteDeliveryCommandValidator : AbstractValidator<CompleteDeliveryCommand>
{
    public CompleteDeliveryCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Complete data is required");

        RuleFor(x => x.Dto.DeliveryId)
            .NotEmpty().WithMessage("Delivery ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
