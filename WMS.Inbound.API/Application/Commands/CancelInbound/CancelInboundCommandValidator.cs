using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.CancelInbound;

public class CancelInboundCommandValidator : AbstractValidator<CancelInboundCommand>
{
    public CancelInboundCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Inbound ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
