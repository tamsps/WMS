using FluentValidation;

namespace WMS.Locations.API.Application.Commands.ActivateLocation;

public class ActivateLocationCommandValidator : AbstractValidator<ActivateLocationCommand>
{
    public ActivateLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
