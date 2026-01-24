using WMS.Domain.Entities;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Mappers;

public static class AuthMapper
{
    public static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}
