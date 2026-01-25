using FluentValidation;

namespace WMS.Products.API.Application.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Product data is required");

        RuleFor(x => x.Dto.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Dto.UOM)
            .NotEmpty().WithMessage("Unit of Measure is required")
            .MaximumLength(20).WithMessage("UOM cannot exceed 20 characters");

        RuleFor(x => x.Dto.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater than or equal to 0");

        RuleFor(x => x.Dto.Length)
            .GreaterThanOrEqualTo(0).WithMessage("Length must be greater than or equal to 0");

        RuleFor(x => x.Dto.Width)
            .GreaterThanOrEqualTo(0).WithMessage("Width must be greater than or equal to 0");

        RuleFor(x => x.Dto.Height)
            .GreaterThanOrEqualTo(0).WithMessage("Height must be greater than or equal to 0");

        RuleFor(x => x.Dto.ReorderLevel)
            .GreaterThanOrEqualTo(0).When(x => x.Dto.ReorderLevel.HasValue)
            .WithMessage("Reorder level must be greater than or equal to 0");

        RuleFor(x => x.Dto.MaxStockLevel)
            .GreaterThanOrEqualTo(0).When(x => x.Dto.MaxStockLevel.HasValue)
            .WithMessage("Max stock level must be greater than or equal to 0")
            .GreaterThan(x => x.Dto.ReorderLevel ?? 0).When(x => x.Dto.MaxStockLevel.HasValue && x.Dto.ReorderLevel.HasValue)
            .WithMessage("Max stock level must be greater than reorder level");

        RuleFor(x => x.Dto.Category)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Dto.Category))
            .WithMessage("Category cannot exceed 100 characters");

        RuleFor(x => x.Dto.Barcode)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Dto.Barcode))
            .WithMessage("Barcode cannot exceed 100 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
