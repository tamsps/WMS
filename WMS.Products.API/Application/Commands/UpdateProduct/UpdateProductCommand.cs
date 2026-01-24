using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public UpdateProductDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
