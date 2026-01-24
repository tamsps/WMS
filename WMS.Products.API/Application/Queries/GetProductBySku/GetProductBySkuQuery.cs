using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetProductBySku;

public class GetProductBySkuQuery : IRequest<Result<ProductDto>>
{
    public string SKU { get; set; } = null!;
}
