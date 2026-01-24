using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.FailDelivery;

public class FailDeliveryCommandValidator : AbstractValidator<FailDeliveryCommand>
{
    public FailDeliveryCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Fail data is required");

        RuleFor(x => x.Dto.DeliveryId)
            .NotEmpty().WithMessage("Delivery ID is required");

        RuleFor(x => x.Dto.FailureReason)
            .NotEmpty().WithMessage("Failure reason is required")
            .MaximumLength(500).WithMessage("Failure reason cannot exceed 500 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
