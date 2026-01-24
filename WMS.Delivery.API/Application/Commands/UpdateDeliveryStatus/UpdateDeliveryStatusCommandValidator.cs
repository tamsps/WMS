using FluentValidation;

namespace WMS.Delivery.API.Application.Commands.UpdateDeliveryStatus;

public class UpdateDeliveryStatusCommandValidator : AbstractValidator<UpdateDeliveryStatusCommand>
{
    public UpdateDeliveryStatusCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Update data is required");

        RuleFor(x => x.Dto.DeliveryId)
            .NotEmpty().WithMessage("Delivery ID is required");

        RuleFor(x => x.Dto.Status)
            .NotEmpty().WithMessage("Status is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
