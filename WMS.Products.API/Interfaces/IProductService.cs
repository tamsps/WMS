using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Interfaces;

public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(Guid id);
    Task<Result<ProductDto>> GetBySKUAsync(string sku);
    Task<Result<PagedResult<ProductDto>>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, string currentUser);
    Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto, string currentUser);
    Task<Result> ActivateAsync(Guid id, string currentUser);
    Task<Result> DeactivateAsync(Guid id, string currentUser);
}
