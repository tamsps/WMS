using FluentValidation;

namespace WMS.Payment.API.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Confirmation data is required");

        RuleFor(x => x.Dto.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
