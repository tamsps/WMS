using FluentValidation;

namespace WMS.Products.API.Application.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Product data is required");

        RuleFor(x => x.Dto.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
