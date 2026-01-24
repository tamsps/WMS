using FluentValidation;

namespace WMS.Auth.API.Application.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Refresh token data is required");

        RuleFor(x => x.Dto.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
