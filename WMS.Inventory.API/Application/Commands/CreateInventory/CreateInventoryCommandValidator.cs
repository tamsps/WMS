using FluentValidation;

namespace WMS.Inventory.API.Application.Commands.CreateInventory;

public class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
{
    public CreateInventoryCommandValidator()
    {
        RuleFor(x => x.Dto.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Dto.LocationId)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.Dto.QuantityOnHand)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity on hand must be 0 or greater");

        RuleFor(x => x.Dto.QuantityReserved)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity reserved must be 0 or greater");

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
    }
}