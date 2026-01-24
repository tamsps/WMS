using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.CreateDelivery;

public class CreateDeliveryCommandValidator : AbstractValidator<CreateDeliveryCommand>
{
    public CreateDeliveryCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Delivery data is required");

        RuleFor(x => x.Dto.OutboundId)
            .NotEmpty().WithMessage("Outbound ID is required");

        RuleFor(x => x.Dto.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
