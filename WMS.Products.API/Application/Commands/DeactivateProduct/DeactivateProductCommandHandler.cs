using MediatR;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.DeactivateProduct;

/// <summary>
/// Handler for deactivating a product
/// Deactivated products cannot be used in new warehouse transactions
/// Historical transactions with deactivated products remain valid
/// </summary>
public class DeactivateProductCommandHandler : IRequestHandler<DeactivateProductCommand, Result>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductCommandHandler(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure("Product not found");
        }

        if (product.Status == ProductStatus.Inactive)
        {
            return Result.Failure("Product is already inactive");
        }

        product.Status = ProductStatus.Inactive;
        product.UpdatedBy = request.CurrentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product deactivated successfully. Existing inventory and historical transactions remain unaffected.");
    }
}
