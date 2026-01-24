using FluentValidation;

namespace WMS.Outbound.API.Application.Commands.PickOutbound;

public class PickOutboundCommandValidator : AbstractValidator<PickOutboundCommand>
{
    public PickOutboundCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Pick data is required");

        RuleFor(x => x.Dto.OutboundId)
            .NotEmpty().WithMessage("Outbound ID is required");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.OutboundItemId)
                .NotEmpty().WithMessage("Outbound item ID is required");

            item.RuleFor(x => x.PickedQuantity)
                .GreaterThan(0).WithMessage("Picked quantity must be greater than 0");
        });

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
