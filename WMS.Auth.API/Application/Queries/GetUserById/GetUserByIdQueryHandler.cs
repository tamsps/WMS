using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Auth.API.Application.Mappers;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly WMSDbContext _context;

    public GetUserByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found");
        }

        return Result<UserDto>.Success(AuthMapper.MapToUserDto(user));
    }
}
