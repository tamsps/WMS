using FluentValidation;

namespace WMS.Locations.API.Application.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Location data is required");

        RuleFor(x => x.Dto.Code)
            .NotEmpty().WithMessage("Location code is required")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Code can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Dto.Zone)
            .NotEmpty().WithMessage("Zone is required")
            .MaximumLength(50).WithMessage("Zone cannot exceed 50 characters");

        RuleFor(x => x.Dto.Aisle)
            .NotEmpty().WithMessage("Aisle is required")
            .MaximumLength(50).WithMessage("Aisle cannot exceed 50 characters");

        RuleFor(x => x.Dto.Rack)
            .NotEmpty().WithMessage("Rack is required")
            .MaximumLength(50).WithMessage("Rack cannot exceed 50 characters");

        RuleFor(x => x.Dto.Shelf)
            .NotEmpty().WithMessage("Shelf is required")
            .MaximumLength(50).WithMessage("Shelf cannot exceed 50 characters");

        RuleFor(x => x.Dto.Bin)
            .NotEmpty().WithMessage("Bin is required")
            .MaximumLength(50).WithMessage("Bin cannot exceed 50 characters");

        RuleFor(x => x.Dto.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");

        RuleFor(x => x.Dto.LocationType)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Dto.LocationType))
            .WithMessage("Location type cannot exceed 50 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
