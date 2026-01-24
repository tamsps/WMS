using FluentValidation;

namespace WMS.Outbound.API.Application.Commands.CreateOutbound;

public class CreateOutboundCommandValidator : AbstractValidator<CreateOutboundCommand>
{
    public CreateOutboundCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Outbound data is required");

        RuleFor(x => x.Dto.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Dto.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.LocationId)
                .NotEmpty().WithMessage("Location ID is required");

            item.RuleFor(x => x.OrderedQuantity)
                .GreaterThan(0).WithMessage("Ordered quantity must be greater than 0");
        });

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
