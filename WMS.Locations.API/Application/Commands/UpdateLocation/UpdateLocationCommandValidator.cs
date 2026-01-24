using FluentValidation;

namespace WMS.Locations.API.Application.Commands.UpdateLocation;

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Location data is required");

        RuleFor(x => x.Dto.Id)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Dto.Capacity)
            .GreaterThanOrEqualTo(0).WithMessage("Capacity cannot be negative");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
