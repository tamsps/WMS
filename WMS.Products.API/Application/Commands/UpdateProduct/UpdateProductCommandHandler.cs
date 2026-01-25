using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Application.Mappers;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Dto.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        // Note: SKU is intentionally not updated here as it is immutable
        // SKU is the unique identifier and cannot be changed after product creation
        
        product.Name = request.Dto.Name;
        product.Description = request.Dto.Description;
        product.Category = request.Dto.Category;
        product.UOM = request.Dto.UOM;
        product.Weight = request.Dto.Weight;
        product.Length = request.Dto.Length;
        product.Width = request.Dto.Width;
        product.Height = request.Dto.Height;
        product.ReorderLevel = request.Dto.ReorderLevel;
        product.MaxStockLevel = request.Dto.MaxStockLevel;
        product.Barcode = request.Dto.Barcode;
        product.UpdatedBy = request.CurrentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(
            ProductMapper.MapToDto(product),
            "Product updated successfully");
    }
}
