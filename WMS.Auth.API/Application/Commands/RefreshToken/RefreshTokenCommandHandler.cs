using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Interfaces;
using WMS.Auth.API.Application.Mappers;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;
using WMS.Auth.API.Interfaces;

namespace WMS.Auth.API.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly WMSDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        WMSDbContext context,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.Dto.RefreshToken, cancellationToken);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Result<LoginResponseDto>.Failure("Refresh token has expired");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("User account is inactive");
        }

        // Get user roles
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // Generate new tokens
        var token = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Update user with new refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = AuthMapper.MapToUserDto(user)
        };

        return Result<LoginResponseDto>.Success(response, "Token refreshed successfully");
    }
}
