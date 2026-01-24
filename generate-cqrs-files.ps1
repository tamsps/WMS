# ?? Automated CQRS File Generator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CQRS File Generator for All Microservices" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# This script generates all CQRS files for remaining microservices
# Based on the WMS.Inbound.API prototype

$ErrorActionPreference = "Continue"

# Helper function to create file with content
function Create-File {
    param(
        [string]$Path,
        [string]$Content
    )
    
    $directory = Split-Path -Path $Path -Parent
    if (!(Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    
    Set-Content -Path $Path -Value $Content -Encoding UTF8
}

# Products API - Simple CRUD
Write-Host "Creating CQRS files for WMS.Products.API..." -ForegroundColor Yellow

# ActivateProduct Command
$activateProductCommand = @"
using MediatR;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.ActivateProduct;

public class ActivateProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
"@

Create-File "WMS.Products.API\Application\Commands\ActivateProduct\ActivateProductCommand.cs" $activateProductCommand

$activateProductHandler = @"
using MediatR;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.ActivateProduct;

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

        product.Status = ProductStatus.Active;
        product.UpdatedBy = request.CurrentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product activated successfully");
    }
}
"@

Create-File "WMS.Products.API\Application\Commands\ActivateProduct\ActivateProductCommandHandler.cs" $activateProductHandler

# DeactivateProduct Command
$deactivateProductCommand = @"
using MediatR;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.DeactivateProduct;

public class DeactivateProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string CurrentUser { get; set; } = null!;
}
"@

Create-File "WMS.Products.API\Application\Commands\DeactivateProduct\DeactivateProductCommand.cs" $deactivateProductCommand

$deactivateProductHandler = @"
using MediatR;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Common.Models;

namespace WMS.Products.API.Application.Commands.DeactivateProduct;

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

        product.Status = ProductStatus.Inactive;
        product.UpdatedBy = request.CurrentUser;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product deactivated successfully");
    }
}
"@

Create-File "WMS.Products.API\Application\Commands\DeactivateProduct\DeactivateProductCommandHandler.cs" $deactivateProductHandler

# Queries for Products
$getProductByIdQuery = @"
using MediatR;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
}
"@

Create-File "WMS.Products.API\Application\Queries\GetProductById\GetProductByIdQuery.cs" $getProductByIdQuery

$getProductByIdHandler = @"
using MediatR;
using WMS.Domain.Interfaces;
using WMS.Domain.Entities;
using WMS.Products.API.Application.Mappers;
using WMS.Products.API.Common.Models;
using WMS.Products.API.DTOs.Product;

namespace WMS.Products.API.Application.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductByIdQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found");
        }

        return Result<ProductDto>.Success(ProductMapper.MapToDto(product));
    }
}
"@

Create-File "WMS.Products.API\Application\Queries\GetProductById\GetProductByIdQueryHandler.cs" $getProductByIdHandler

Write-Host "? WMS.Products.API CQRS files created" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? File Generation Complete!" -ForegroundColor Green
Write-Host "Next: Run the solution build to verify" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
