using MediatR;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.ActivateProduct;

public class ActivateProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
