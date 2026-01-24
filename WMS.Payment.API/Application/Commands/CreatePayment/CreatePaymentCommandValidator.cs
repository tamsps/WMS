using FluentValidation;

namespace WMS.Payment.API.Application.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Payment data is required");

        RuleFor(x => x.Dto.PaymentType)
            .NotEmpty().WithMessage("Payment type is required");

        RuleFor(x => x.Dto.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Dto.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency code must be 3 characters");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
