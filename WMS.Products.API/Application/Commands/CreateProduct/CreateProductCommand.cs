using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Result<ProductDto>>
{
    public CreateProductDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
