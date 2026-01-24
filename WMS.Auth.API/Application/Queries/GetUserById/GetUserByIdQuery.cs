using MediatR;
using WMS.Auth.API.Common.Models;
using WMS.Auth.API.DTOs.Auth;

namespace WMS.Auth.API.Application.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid Id { get; set; }
}
