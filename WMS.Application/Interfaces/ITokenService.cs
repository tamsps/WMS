using WMS.Domain.Entities;

namespace WMS.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}
