using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Auth;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly WMSDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(WMSDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid username or password");
        }

        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Result<LoginResponseDto>.Failure("Invalid username or password");
        }

        // Update last login
        user.LastLoginDate = DateTime.UtcNow;
        
        // Generate tokens
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Store refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Roles = roles
        };

        var response = new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };

        return Result<LoginResponseDto>.Success(response, "Login successful");
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterDto dto)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return Result<UserDto>.Failure("Username already exists");
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return Result<UserDto>.Failure("Email already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true,
            CreatedBy = "System"
        };

        // Assign default role (WarehouseStaff)
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "WarehouseStaff");
        if (defaultRole != null)
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = defaultRole.Id,
                AssignedBy = "System",
                AssignedAt = DateTime.UtcNow
            });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Roles = defaultRole != null ? new List<string> { defaultRole.Name } : new List<string>()
        };

        return Result<UserDto>.Success(userDto, "Registration successful");
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // Validate refresh token exists in database
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken && u.IsActive);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Result<LoginResponseDto>.Failure("Refresh token has expired");
        }

        // Generate new tokens
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Update refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Roles = roles
        };

        var response = new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };

        return Result<LoginResponseDto>.Success(response, "Token refreshed successfully");
    }

    public async Task<Result> LogoutAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return Result.Failure("User not found");
        }

        // Clear refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        
        await _context.SaveChangesAsync();

        return Result.Success("Logged out successfully");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
