using FluentValidation;

namespace WMS.Locations.API.Application.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Location data is required");

        RuleFor(x => x.Dto.Code)
            .NotEmpty().WithMessage("Location code is required")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Dto.Capacity)
            .GreaterThanOrEqualTo(0).WithMessage("Capacity cannot be negative");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
