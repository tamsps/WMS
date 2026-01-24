using FluentValidation;

namespace WMS.Inbound.API.Application.Commands.ReceiveInbound;

public class ReceiveInboundCommandValidator : AbstractValidator<ReceiveInboundCommand>
{
    public ReceiveInboundCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Receive data is required");

        RuleFor(x => x.Dto.InboundId)
            .NotEmpty().WithMessage("Inbound ID is required");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("At least one item is required to receive");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.InboundItemId)
                .NotEmpty().WithMessage("Inbound item ID is required");

            item.RuleFor(i => i.ReceivedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Received quantity cannot be negative");

            item.RuleFor(i => i.DamagedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Damaged quantity cannot be negative");

            item.RuleFor(i => i)
                .Must(i => (i.DamagedQuantity ?? 0) <= i.ReceivedQuantity)
                .WithMessage("Damaged quantity cannot exceed received quantity");
        });

        RuleFor(x => x.CurrentUser)
            .NotEmpty().WithMessage("Current user is required");
    }
}
