using MediatR;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Commands.Register;

public class RegisterCommand : IRequest<Result<UserDto>>
{
    public RegisterDto Dto { get; set; } = null!;
}
