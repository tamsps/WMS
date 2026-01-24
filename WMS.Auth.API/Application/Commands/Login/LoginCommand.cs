using MediatR;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Commands.Login;

public class LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public LoginDto Dto { get; set; } = null!;
}
