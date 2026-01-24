using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Products.API.Application.Mappers;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetProductBySku;

public class GetProductBySkuQueryHandler : IRequestHandler<GetProductBySkuQuery, Result<ProductDto>>
{
    private readonly WMSDbContext _context;

    public GetProductBySkuQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDto>> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == request.SKU, cancellationToken);

        if (product == null)
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' not found");
        }

        return Result<ProductDto>.Success(ProductMapper.MapToDto(product));
    }
}
