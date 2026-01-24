using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Products.API.Application.Mappers;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        WMSDbContext context,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == request.Dto.SKU, cancellationToken);

        if (existingProduct != null)
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.Dto.SKU}' already exists");
        }

        var product = new Product
        {
            SKU = request.Dto.SKU,
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Category = request.Dto.Category,
            UOM = request.Dto.UOM,
            Weight = request.Dto.Weight,
            Length = request.Dto.Length,
            Width = request.Dto.Width,
            Height = request.Dto.Height,
            ReorderLevel = request.Dto.ReorderLevel,
            MaxStockLevel = request.Dto.MaxStockLevel,
            Status = ProductStatus.Active,
            Barcode = request.Dto.Barcode,
            CreatedBy = request.CurrentUser
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(
            ProductMapper.MapToDto(product),
            "Product created successfully");
    }
}
