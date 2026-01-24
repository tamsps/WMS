using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommandValidator : AbstractValidator<CreateInboundCommand>
{
    public CreateInboundCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Inbound data is required");

        RuleFor(x => x.Dto.SupplierName)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Supplier name cannot exceed 200 characters");

        RuleFor(x => x.Dto.ExpectedDate)
            .NotEmpty().WithMessage("Expected date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Expected date cannot be in the past");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product is required");

            item.RuleFor(i => i.LocationId)
                .NotEmpty().WithMessage("Location is required");

            item.RuleFor(i => i.ExpectedQuantity)
                .GreaterThan(0).WithMessage("Expected quantity must be greater than 0");
        });

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
