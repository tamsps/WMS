using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Auth.API.Application.Mappers;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;
using WMS.Auth.API.Interfaces;

namespace WMS.Auth.API.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly WMSDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        WMSDbContext context,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user with roles
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Dto.Username, cancellationToken);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid username or password");
        }

        // Verify password (using simple comparison for now - should use proper hashing)
        if (!VerifyPassword(request.Dto.Password, user.PasswordHash))
        {
            return Result<LoginResponseDto>.Failure("Invalid username or password");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("User account is inactive");
        }

        // Get user roles
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // Generate tokens
        var token = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Update user with refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LastLoginDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
            User = AuthMapper.MapToUserDto(user)
        };

        return Result<LoginResponseDto>.Success(response, "Login successful");
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // For now, simple comparison (in production, use BCrypt or similar)
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
