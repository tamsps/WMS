using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetActiveProducts;

/// <summary>
/// Query to retrieve only active products (convenience query for transaction operations)
/// </summary>
public class GetActiveProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
