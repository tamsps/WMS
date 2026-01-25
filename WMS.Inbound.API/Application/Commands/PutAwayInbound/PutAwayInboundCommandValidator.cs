using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.PutAwayInbound;

public class PutAwayInboundCommandValidator : AbstractValidator<PutAwayInboundCommand>
{
    public PutAwayInboundCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Inbound ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
