using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Repositories;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Commands.CreateOutbound;
using WMS.Outbound.API.Application.Commands.PickOutbound;
using WMS.Outbound.API.Application.Commands.ShipOutbound;
using WMS.Outbound.API.DTOs.Outbound;
using WMS.Tests.Fixtures;
using WMS.Tests.Helpers;

namespace WMS.Tests.Outbound;

/// <summary>
/// Integration tests for Outbound API
/// Tests pick/ship flow, inventory reservation, concurrency, and payment validation
/// </summary>
public class OutboundCommandHandlerTests : IDisposable
{
    private readonly WMSDbContext _context;
    private readonly IRepository<WMS.Domain.Entities.Outbound> _outboundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OutboundCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _outboundRepository = new Repository<WMS.Domain.Entities.Outbound>(_context);
        _unitOfWork = new UnitOfWork(_context);
        TestDataGenerator.ResetCounters();
    }

    [Fact]
    public async Task CreateOutbound_WithValidData_ShouldSucceed()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation();
        _context.Products.Add(product);
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var dto = new CreateOutboundDto
        {
            OrderNumber = "SO-001",
            CustomerName = "Test Customer",
            CustomerCode = "CUST-001",
            ShippingAddress = "123 Test St",
            Items = new List<CreateOutboundItemDto>
            {
                new CreateOutboundItemDto
                {
                    ProductId = product.Id,
                    LocationId = location.Id,
                    OrderedQuantity = 10
                }
            }
        };

        var command = new CreateOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new CreateOutboundCommandHandler(_context, _outboundRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(OutboundStatus.Pending.ToString());
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task PickOutbound_ShouldReserveInventory()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation();
        var inventory = TestDataGenerator.GenerateInventory(product.Id, location.Id, 100m, 0m);
        var outbound = TestDataGenerator.GenerateOutbound();
        var outboundItem = TestDataGenerator.GenerateOutboundItem(outbound.Id, product.Id, location.Id, 10m);

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inventories.Add(inventory);
        _context.Outbounds.Add(outbound);
        _context.OutboundItems.Add(outboundItem);
        await _context.SaveChangesAsync();

        var dto = new PickOutboundDto
        {
            OutboundId = outbound.Id,
            Items = new List<PickOutboundItemDto>
            {
                new PickOutboundItemDto
                {
                    OutboundItemId = outboundItem.Id,
                    PickedQuantity = 10
                }
            }
        };

        var command = new PickOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new PickOutboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(OutboundStatus.Picked.ToString());

        // Verify inventory was reserved
        var updatedInventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.LocationId == location.Id);
        updatedInventory.Should().NotBeNull();
        updatedInventory!.QuantityReserved.Should().Be(10);
        updatedInventory.QuantityOnHand.Should().Be(100); // Still on hand, not deducted yet
    }

    [Fact]
    public async Task PickOutbound_WithInsufficientInventory_ShouldFail()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation();
        var inventory = TestDataGenerator.GenerateInventory(product.Id, location.Id, 5m, 0m); // Only 5 available
        var outbound = TestDataGenerator.GenerateOutbound();
        var outboundItem = TestDataGenerator.GenerateOutboundItem(outbound.Id, product.Id, location.Id, 10m);

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inventories.Add(inventory);
        _context.Outbounds.Add(outbound);
        _context.OutboundItems.Add(outboundItem);
        await _context.SaveChangesAsync();

        var dto = new PickOutboundDto
        {
            OutboundId = outbound.Id,
            Items = new List<PickOutboundItemDto>
            {
                new PickOutboundItemDto
                {
                    OutboundItemId = outboundItem.Id,
                    PickedQuantity = 10  // Trying to pick 10 but only 5 available
                }
            }
        };

        var command = new PickOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var handler = new PickOutboundCommandHandler(_context, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Insufficient"));
    }

    [Fact]
    public async Task ShipOutbound_ShouldDeductInventory()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        product.Length = 10m;
        product.Width = 10m;
        product.Height = 10m;

        var location = TestDataGenerator.GenerateLocation(capacity: 1000m);
        var inventory = TestDataGenerator.GenerateInventory(product.Id, location.Id, 100m, 10m); // 10 reserved
        var outbound = TestDataGenerator.GenerateOutbound();
        outbound.Status = OutboundStatus.Picked;

        var outboundItem = TestDataGenerator.GenerateOutboundItem(outbound.Id, product.Id, location.Id, 10m);
        outboundItem.PickedQuantity = 10m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inventories.Add(inventory);
        _context.Outbounds.Add(outbound);
        _context.OutboundItems.Add(outboundItem);
        await _context.SaveChangesAsync();

        var dto = new ShipOutboundDto
        {
            OutboundId = outbound.Id
        };

        var command = new ShipOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var transactionRepo = new Repository<InventoryTransaction>(_context);
        var handler = new ShipOutboundCommandHandler(_context, transactionRepo, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(OutboundStatus.Shipped.ToString());

        // Verify inventory was deducted
        var updatedInventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.LocationId == location.Id);
        updatedInventory.Should().NotBeNull();
        updatedInventory!.QuantityOnHand.Should().Be(90); // 100 - 10 = 90
        updatedInventory.QuantityReserved.Should().Be(0); // Reserved cleared

        // Verify inventory transaction was created
        var transaction = await _context.InventoryTransactions
            .FirstOrDefaultAsync(t => t.ProductId == product.Id);
        transaction.Should().NotBeNull();
        transaction!.Quantity.Should().Be(-10); // Negative for outbound
        transaction.BalanceBefore.Should().Be(100);
        transaction.BalanceAfter.Should().Be(90);

        // Verify location occupancy was updated
        var updatedLocation = await _context.Locations.FindAsync(location.Id);
        updatedLocation!.CurrentOccupancy.Should().BeLessThan(location.CurrentOccupancy);
    }

    [Fact]
    public async Task ShipOutbound_WithPrepaidPaymentNotConfirmed_ShouldFail()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        var location = TestDataGenerator.GenerateLocation();
        var inventory = TestDataGenerator.GenerateInventory(product.Id, location.Id, 100m, 10m);
        var outbound = TestDataGenerator.GenerateOutbound();
        outbound.Status = OutboundStatus.Picked;

        var payment = TestDataGenerator.GeneratePayment(outbound.Id, PaymentType.Prepaid);
        payment.Status = PaymentStatus.Pending; // Not confirmed

        outbound.PaymentId = payment.Id;

        var outboundItem = TestDataGenerator.GenerateOutboundItem(outbound.Id, product.Id, location.Id, 10m);
        outboundItem.PickedQuantity = 10m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inventories.Add(inventory);
        _context.Payments.Add(payment);
        _context.Outbounds.Add(outbound);
        _context.OutboundItems.Add(outboundItem);
        await _context.SaveChangesAsync();

        var dto = new ShipOutboundDto { OutboundId = outbound.Id };
        var command = new ShipOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var transactionRepo = new Repository<InventoryTransaction>(_context);
        var handler = new ShipOutboundCommandHandler(_context, transactionRepo, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Payment") || e.Contains("confirmed"));
    }

    [Fact]
    public async Task ShipOutbound_WithCODPayment_ShouldAllowShipping()
    {
        // Arrange
        var product = TestDataGenerator.GenerateProduct();
        product.Length = 10m;
        product.Width = 10m;
        product.Height = 10m;

        var location = TestDataGenerator.GenerateLocation();
        var inventory = TestDataGenerator.GenerateInventory(product.Id, location.Id, 100m, 10m);
        var outbound = TestDataGenerator.GenerateOutbound();
        outbound.Status = OutboundStatus.Picked;

        var payment = TestDataGenerator.GeneratePayment(outbound.Id, PaymentType.COD);
        payment.Status = PaymentStatus.Pending; // Pending is OK for COD

        outbound.PaymentId = payment.Id;

        var outboundItem = TestDataGenerator.GenerateOutboundItem(outbound.Id, product.Id, location.Id, 10m);
        outboundItem.PickedQuantity = 10m;

        _context.Products.Add(product);
        _context.Locations.Add(location);
        _context.Inventories.Add(inventory);
        _context.Payments.Add(payment);
        _context.Outbounds.Add(outbound);
        _context.OutboundItems.Add(outboundItem);
        await _context.SaveChangesAsync();

        var dto = new ShipOutboundDto { OutboundId = outbound.Id };
        var command = new ShipOutboundCommand { Dto = dto, CurrentUser = "TestUser" };
        var transactionRepo = new Repository<InventoryTransaction>(_context);
        var handler = new ShipOutboundCommandHandler(_context, transactionRepo, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // COD allows shipping without payment confirmation
        result.Data!.Status.Should().Be(OutboundStatus.Shipped.ToString());
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
