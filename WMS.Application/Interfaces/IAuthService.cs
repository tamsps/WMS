using WMS.Application.Common.Models;
using WMS.Application.DTOs.Auth;

namespace WMS.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<UserDto>> RegisterAsync(RegisterDto dto);
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<Result> LogoutAsync(string username);
}
