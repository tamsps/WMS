using MediatR;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.DeactivateProduct;

public class DeactivateProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
