using FluentValidation;

namespace WMS.Outbound.API.Application.Commands.ShipOutbound;

public class ShipOutboundCommandValidator : AbstractValidator<ShipOutboundCommand>
{
    public ShipOutboundCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Ship data is required");

        RuleFor(x => x.Dto.OutboundId)
            .NotEmpty().WithMessage("Outbound ID is required");

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
