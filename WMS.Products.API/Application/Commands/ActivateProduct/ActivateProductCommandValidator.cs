using FluentValidation;

namespace WMS.Products.API.Application.Commands.ActivateProduct;

public class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
