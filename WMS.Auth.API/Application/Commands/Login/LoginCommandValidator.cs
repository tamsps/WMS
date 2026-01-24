using FluentValidation;

namespace WMS.Auth.API.Application.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Login data is required");

        RuleFor(x => x.Dto.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Dto.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
