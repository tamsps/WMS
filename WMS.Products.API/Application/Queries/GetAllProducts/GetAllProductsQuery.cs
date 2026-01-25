using MediatR;
using WMS.Domain.Enums;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public ProductStatus? Status { get; set; }
}
