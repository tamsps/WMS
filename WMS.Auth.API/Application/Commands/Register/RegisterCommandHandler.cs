using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Auth.API.Application.Mappers;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        WMSDbContext context,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Dto.Username, cancellationToken);

        if (existingUser != null)
        {
            return Result<UserDto>.Failure("Username already exists");
        }

        // Check if email already exists
        var existingEmail = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Dto.Email, cancellationToken);

        if (existingEmail != null)
        {
            return Result<UserDto>.Failure("Email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Dto.Password);

        // Create user
        var user = new User
        {
            Username = request.Dto.Username,
            Email = request.Dto.Email,
            PasswordHash = passwordHash,
            FirstName = request.Dto.FirstName,
            LastName = request.Dto.LastName,
            IsActive = true,
            CreatedBy = "System"
        };

        await _userRepository.AddAsync(user, cancellationToken);

        // Assign default role (User)
        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);

        if (defaultRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id,
                AssignedBy = "System",
                AssignedAt = DateTime.UtcNow
            };

            _context.Set<UserRole>().Add(userRole);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload user with roles
        var createdUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        return Result<UserDto>.Success(
            AuthMapper.MapToUserDto(createdUser!),
            "User registered successfully");
    }
}
