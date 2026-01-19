using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Product;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        WMSDbContext context,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductDto>> GetBySKUAsync(string sku)
    {
        var product = await _productRepository.FirstOrDefaultAsync(p => p.SKU == sku);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<PagedResult<ProductDto>>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.SKU.Contains(searchTerm) || 
                p.Name.Contains(searchTerm) ||
                p.Description.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.SKU)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<ProductDto>
        {
            Items = products.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<ProductDto>>.Success(result);
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, string currentUser)
    {
        // Validate SKU uniqueness
        var exists = await _productRepository.ExistsAsync(p => p.SKU == dto.SKU);
        if (exists)
        {
            return Result<ProductDto>.Failure($"Product with SKU '{dto.SKU}' already exists");
        }

        var product = new Product
        {
            SKU = dto.SKU,
            Name = dto.Name,
            Description = dto.Description,
            Status = ProductStatus.Active,
            UOM = dto.UOM,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            Barcode = dto.Barcode,
            Category = dto.Category,
            ReorderLevel = dto.ReorderLevel,
            MaxStockLevel = dto.MaxStockLevel,
            CreatedBy = currentUser
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProductDto>.Success(MapToDto(product), "Product created successfully");
    }

    public async Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto, string currentUser)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.UOM = dto.UOM;
        product.Weight = dto.Weight;
        product.Length = dto.Length;
        product.Width = dto.Width;
        product.Height = dto.Height;
        product.Barcode = dto.Barcode;
        product.Category = dto.Category;
        product.ReorderLevel = dto.ReorderLevel;
        product.MaxStockLevel = dto.MaxStockLevel;
        product.UpdatedBy = currentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProductDto>.Success(MapToDto(product), "Product updated successfully");
    }

    public async Task<Result> ActivateAsync(Guid id, string currentUser)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }

        product.Status = ProductStatus.Active;
        product.UpdatedBy = currentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Product activated successfully");
    }

    public async Task<Result> DeactivateAsync(Guid id, string currentUser)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }

        product.Status = ProductStatus.Inactive;
        product.UpdatedBy = currentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Product deactivated successfully");
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Status = product.Status.ToString(),
            UOM = product.UOM,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            Barcode = product.Barcode,
            Category = product.Category,
            ReorderLevel = product.ReorderLevel,
            MaxStockLevel = product.MaxStockLevel,
            CreatedAt = product.CreatedAt
        };
    }
}
