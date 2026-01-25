using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Products.API.Application.Mappers;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetActiveProducts;

/// <summary>
/// Handler for retrieving only active products
/// Commonly used by transaction services to ensure only active products are available
/// </summary>
public class GetActiveProductsQueryHandler : IRequestHandler<GetActiveProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly WMSDbContext _context;

    public GetActiveProductsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetActiveProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Where(p => p.Status == ProductStatus.Active)
            .AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                p.SKU.Contains(request.SearchTerm) ||
                p.Name.Contains(request.SearchTerm) ||
                (p.Description != null && p.Description.Contains(request.SearchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductDto>
        {
            Items = products.Select(ProductMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<ProductDto>>.Success(result);
    }
}
