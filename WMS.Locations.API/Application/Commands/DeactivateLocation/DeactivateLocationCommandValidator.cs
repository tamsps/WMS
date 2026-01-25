using FluentValidation;

namespace WMS.Locations.API.Application.Commands.DeactivateLocation;

public class DeactivateLocationCommandValidator : AbstractValidator<DeactivateLocationCommand>
{
    public DeactivateLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
