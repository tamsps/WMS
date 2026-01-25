using FluentAssertions;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Repositories;
using WMS.Inbound.API.Application.Commands.CreateInbound;
using WMS.Inbound.API.Application.Commands.ReceiveInbound;
using WMS.Inbound.API.DTOs.Inbound;
using WMS.Tests.Fixtures;
using WMS.Tests.Helpers;

namespace WMS.Tests.Inbound;

/// <summary>
/// Integration tests for Inbound API
/// Tests atomic transactions, capacity validation, and inventory consistency
/// </summary>
public class InboundCommandHandlerTests : IDisposable
{
    private readonly WMSDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public InboundCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _unitOfWork = new UnitOfWork(_context);
        TestDataGenerator.ResetCounters();
    }

    [Fact]
    public async Task CreateInbound_WithValidData_ShouldSucceed()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation();
        _context.Products.Add(product);
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var dto = new CreateInboundDto
        {
            ReferenceNumber = "PO-001",
            ExpectedDate = DateTime.UtcNow.AddDays(3),
            SupplierName = "Test Supplier",
            SupplierCode = "SUP-001",
            Notes = "Test inbound",
            Items = new List<CreateInboundItemDto>
            {
                new CreateInboundItemDto
                {
                    ProductId = product.Id,
                    LocationId = location.Id,
                    ExpectedQuantity = 100
                }
            }
        };

        var command = new CreateInboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new CreateInboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(InboundStatus.Pending.ToString());
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().ExpectedQuantity.Should().Be(100);
    }

    [Fact]
    public async Task ReceiveInbound_ShouldUpdateInventoryAtomically()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation(capacity: 1000m);
        var inbound = TestDataGenerator.GenerateInbound();
        var inboundItem = TestDataGenerator.GenerateInboundItem(inbound.Id, product.Id, location.Id, 100m);

        product.Length = 10m;
        product.Width = 10m;
        product.Height = 10m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inbounds.Add(inbound);
        _context.InboundItems.Add(inboundItem);
        await _context.SaveChangesAsync();

        var dto = new ReceiveInboundDto
        {
            InboundId = inbound.Id,
            Items = new List<ReceiveInboundItemDto>
            {
                new ReceiveInboundItemDto
                {
                    InboundItemId = inboundItem.Id,
                    ReceivedQuantity = 100,
                    DamagedQuantity = 0
                }
            }
        };

        var command = new ReceiveInboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new ReceiveInboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(InboundStatus.Received.ToString());

        // Verify inventory was created
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.LocationId == location.Id);
        inventory.Should().NotBeNull();
        inventory!.QuantityOnHand.Should().Be(100);

        // Verify inventory transaction was created
        var transaction = await _context.InventoryTransactions
            .FirstOrDefaultAsync(t => t.ProductId == product.Id);
        transaction.Should().NotBeNull();
        transaction!.Quantity.Should().Be(100);
        transaction.BalanceAfter.Should().Be(100);

        // Verify location occupancy was updated
        var updatedLocation = await _context.Locations.FindAsync(location.Id);
        updatedLocation!.CurrentOccupancy.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReceiveInbound_WithDamagedItems_ShouldNotIncreaseInventory()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation(capacity: 1000m);
        var inbound = TestDataGenerator.GenerateInbound();
        var inboundItem = TestDataGenerator.GenerateInboundItem(inbound.Id, product.Id, location.Id, 100m);

        product.Length = 10m;
        product.Width = 10m;
        product.Height = 10m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inbounds.Add(inbound);
        _context.InboundItems.Add(inboundItem);
        await _context.SaveChangesAsync();

        var dto = new ReceiveInboundDto
        {
            InboundId = inbound.Id,
            Items = new List<ReceiveInboundItemDto>
            {
                new ReceiveInboundItemDto
                {
                    InboundItemId = inboundItem.Id,
                    ReceivedQuantity = 100,
                    DamagedQuantity = 10  // 10 damaged items
                }
            }
        };

        var command = new ReceiveInboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new ReceiveInboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.LocationId == location.Id);
        
        inventory.Should().NotBeNull();
        inventory!.QuantityOnHand.Should().Be(90); // Only good quantity (100 - 10 = 90)
    }

    [Fact]
    public async Task ReceiveInbound_WithInsufficientCapacity_ShouldFail()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation(capacity: 0.001m); // Very small capacity
        var inbound = TestDataGenerator.GenerateInbound();
        var inboundItem = TestDataGenerator.GenerateInboundItem(inbound.Id, product.Id, location.Id, 100m);

        product.Length = 100m;  // Large product
        product.Width = 100m;
        product.Height = 100m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inbounds.Add(inbound);
        _context.InboundItems.Add(inboundItem);
        await _context.SaveChangesAsync();

        var dto = new ReceiveInboundDto
        {
            InboundId = inbound.Id,
            Items = new List<ReceiveInboundItemDto>
            {
                new ReceiveInboundItemDto
                {
                    InboundItemId = inboundItem.Id,
                    ReceivedQuantity = 100,
                    DamagedQuantity = 0
                }
            }
        };

        var command = new ReceiveInboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new ReceiveInboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("capacity"));
    }

    [Fact]
    public async Task ReceiveInbound_WithInactiveProduct_ShouldFail()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct(ProductStatus.Inactive);
        var location = TestDataGenerator.GenerateLocation();
        var inbound = TestDataGenerator.GenerateInbound();
        var inboundItem = TestDataGenerator.GenerateInboundItem(inbound.Id, product.Id, location.Id, 100m);

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inbounds.Add(inbound);
        _context.InboundItems.Add(inboundItem);
        await _context.SaveChangesAsync();

        var dto = new ReceiveInboundDto
        {
            InboundId = inbound.Id,
            Items = new List<ReceiveInboundItemDto>
            {
                new ReceiveInboundItemDto
                {
                    InboundItemId = inboundItem.Id,
                    ReceivedQuantity = 100,
                    DamagedQuantity = 0
                }
            }
        };

        var command = new ReceiveInboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new ReceiveInboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("inactive") || e.Contains("not active"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
