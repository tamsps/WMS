using MediatR;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.ActivateProduct;

/// <summary>
/// Handler for activating a product
/// Activated products can participate in new warehouse transactions
/// </summary>
public class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand, Result>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }

        if (product.Status == ProductStatus.Active)
        {
            return Result.Failure("Product is already active");
        }

        product.Status = ProductStatus.Active;
        product.UpdatedBy = request.CurrentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product activated successfully");
    }
}
