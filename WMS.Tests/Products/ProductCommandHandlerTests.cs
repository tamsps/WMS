using FluentAssertions;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Repositories;
using WMS.Domain.Interfaces;
using WMS.Products.API.Application.Commands.CreateProduct;
using WMS.Products.API.Application.Commands.UpdateProduct;
using WMS.Products.API.Application.Commands.ActivateProduct;
using WMS.Products.API.Application.Commands.DeactivateProduct;
using WMS.Products.API.Application.Queries.GetProductById;
using WMS.Products.API.Application.Queries.GetAllProducts;
using WMS.Products.API.DTOs.Product;
using WMS.Tests.Fixtures;
using WMS.Tests.Helpers;

namespace WMS.Tests.Products;

/// <summary>
/// Unit tests for Product API Command Handlers
/// Tests product CRUD operations, activation/deactivation, and immutability of SKU
/// </summary>
public class ProductCommandHandlerTests : IDisposable
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _productRepository = new Repository<Product>(_context);
        _unitOfWork = new UnitOfWork(_context);
        TestDataGenerator.ResetCounters();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "TEST-001",
            Name = "Test Product",
            Description = "Test Description",
            UOM = "PCS",
            Weight = 1.5m,
            Length = 10m,
            Width = 10m,
            Height = 10m,
            Category = "Electronics"
        };

        var command = new CreateProductCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new CreateProductCommandHandler(_context, _productRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SKU.Should().Be("TEST-001");
        result.Data.Name.Should().Be("Test Product");
        result.Data.Status.Should().Be(ProductStatus.Active.ToString());
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSKU_ShouldFail()
    {
        // Arrange
        var existingProduct = TestDataGenerator.GenerateProduct();
        existingProduct.SKU = "TEST-001";
        await _productRepository.AddAsync(existingProduct);
        await _unitOfWork.SaveChangesAsync();

        var dto = new CreateProductDto
        {
            SKU = "TEST-001", // Duplicate SKU
            Name = "Another Product",
            Description = "Test",
            UOM = "PCS",
            Weight = 1m,
            Length = 10m,
            Width = 10m,
            Height = 10m
        };

        var command = new CreateProductCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new CreateProductCommandHandler(_context, _productRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateProduct_ShouldNotChangeSKU()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var originalSKU = product.SKU;
        var dto = new UpdateProductDto
        {
            Id = product.Id,
            Name = "Updated Product Name",
            Description = "Updated Description",
            UOM = "BOX",
            Weight = 2.5m,
            Length = 20m,
            Width = 20m,
            Height = 20m
        };

        var command = new UpdateProductCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.SKU.Should().Be(originalSKU); // SKU should remain unchanged
        result.Data.Name.Should().Be("Updated Product Name");
    }

    [Fact]
    public async Task ActivateProduct_ShouldChangeStatusToActive()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct(ProductStatus.Inactive);
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var command = new ActivateProductCommand { Id = product.Id, CurrentUser = "TestUser" };
        var handler = new ActivateProductCommandHandler(_productRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify status changed
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct!.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public async Task DeactivateProduct_ShouldChangeStatusToInactive()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct(ProductStatus.Active);
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var command = new DeactivateProductCommand { Id = product.Id, CurrentUser = "TestUser" };
        var handler = new DeactivateProductCommandHandler(_productRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify status changed
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct!.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var query = new GetProductByIdQuery { Id = product.Id };
        var handler = new GetProductByIdQueryHandler(_productRepository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(product.Id);
        result.Data.SKU.Should().Be(product.SKU);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            var product = TestDataGenerator.GenerateProduct();
            await _productRepository.AddAsync(product);
        }
        await _unitOfWork.SaveChangesAsync();

        var query = new GetAllProductsQuery { PageNumber = 1, PageSize = 10 };
        var handler = new GetAllProductsQueryHandler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(15);
        result.Data.TotalPages.Should().Be(2);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
