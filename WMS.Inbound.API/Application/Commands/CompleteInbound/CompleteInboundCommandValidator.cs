using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.CompleteInbound;

public class CompleteInboundCommandValidator : AbstractValidator<CompleteInboundCommand>
{
    public CompleteInboundCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Inbound ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
