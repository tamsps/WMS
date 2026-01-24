using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<UserDto>> RegisterAsync(RegisterDto dto);
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<Result> LogoutAsync(string username);
}
