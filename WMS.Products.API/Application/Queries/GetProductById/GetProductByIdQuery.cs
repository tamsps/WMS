using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
}
