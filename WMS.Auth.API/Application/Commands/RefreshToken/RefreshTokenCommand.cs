using MediatR;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
{
    public RefreshTokenDto Dto { get; set; } = null!;
}
