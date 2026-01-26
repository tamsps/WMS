using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WMS.Delivery.API.Application.Commands.ProcessDeliveryWebhook;
using WMS.Delivery.API.DTOs.Delivery;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Repositories;
using WMS.Domain.Interfaces;
using WMS.Tests.Fixtures;
using WMS.Tests.Helpers;

namespace WMS.Tests.Delivery;

/// <summary>
/// Integration tests for Delivery Webhook processing
/// Tests idempotency, status transitions, and partner event tracking
/// </summary>
public class DeliveryWebhookTests : IDisposable
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Mock<ILogger<ProcessDeliveryWebhookCommandHandler>> _loggerMock;

    public DeliveryWebhookTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _unitOfWork = new UnitOfWork(_context);
        _loggerMock = new Mock<ILogger<ProcessDeliveryWebhookCommandHandler>>();
        TestDataGenerator.ResetCounters();
    }

    [Fact]
    public async Task ProcessWebhook_FirstTime_ShouldUpdateDeliveryStatus()
    {
        // Arrange
        var outbound = TestDataGenerator.GenerateOutbound();
        var delivery = TestDataGenerator.GenerateDelivery(outbound.Id, "1Z999AA10123456784");
        delivery.Status = DeliveryStatus.Pending;

        _context.Outbounds.Add(outbound);
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

        var dto = new DeliveryWebhookDto
        {
            TrackingNumber = "1Z999AA10123456784",
            PartnerEventId = "fedex_evt_001",
            Status = "InTransit",
            CurrentLocation = "Memphis, TN",
            EventTimestamp = DateTime.UtcNow,
            EventData = "{\"location\": \"Memphis, TN\"}",
            Notes = "Package in transit"
        };

        var command = new ProcessDeliveryWebhookCommand { Dto = dto };
        var handler = new ProcessDeliveryWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedDelivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.TrackingNumber == "1Z999AA10123456784");

        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Status.Should().Be(DeliveryStatus.InTransit);
        updatedDelivery.PickupDate.Should().NotBeNull();
        updatedDelivery.DeliveryEvents.Should().HaveCount(1);
        updatedDelivery.DeliveryEvents.First().PartnerEventId.Should().Be("fedex_evt_001");
        updatedDelivery.DeliveryEvents.First().IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhook_DuplicateEvent_ShouldIgnoreAndLogDuplicate()
    {
        // Arrange
        var outbound = TestDataGenerator.GenerateOutbound();
        var delivery = TestDataGenerator.GenerateDelivery(outbound.Id, "1Z999AA10123456784");

        _context.Outbounds.Add(outbound);
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

        var handler = new ProcessDeliveryWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // First webhook
        var dto1 = new DeliveryWebhookDto
        {
            TrackingNumber = "1Z999AA10123456784",
            PartnerEventId = "fedex_evt_duplicate",
            Status = "InTransit",
            CurrentLocation = "Memphis, TN",
            EventData = "{}"
        };
        var command1 = new ProcessDeliveryWebhookCommand { Dto = dto1 };
        await handler.Handle(command1, CancellationToken.None);

        // Second webhook (duplicate)
        var dto2 = new DeliveryWebhookDto
        {
            TrackingNumber = "1Z999AA10123456784",
            PartnerEventId = "fedex_evt_duplicate", // Same event ID
            Status = "InTransit",
            CurrentLocation = "Memphis, TN",
            EventData = "{}"
        };
        var command2 = new ProcessDeliveryWebhookCommand { Dto = dto2 };

        // Act
        var result = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("already processed");

        var updatedDelivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.TrackingNumber == "1Z999AA10123456784");

        updatedDelivery!.DeliveryEvents.Should().HaveCount(2);

        var duplicateEvent = updatedDelivery.DeliveryEvents
            .FirstOrDefault(e => e.EventType == "WebhookDuplicate");

        duplicateEvent.Should().NotBeNull();
        duplicateEvent!.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessWebhook_DeliveredStatus_ShouldSetActualDeliveryDate()
    {
        // Arrange
        var outbound = TestDataGenerator.GenerateOutbound();
        var delivery = TestDataGenerator.GenerateDelivery(outbound.Id, "1Z999AA10123456784");
        delivery.Status = DeliveryStatus.InTransit;

        _context.Outbounds.Add(outbound);
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

        var deliveryTimestamp = DateTime.UtcNow;
        var dto = new DeliveryWebhookDto
        {
            TrackingNumber = "1Z999AA10123456784",
            PartnerEventId = "fedex_evt_delivered",
            Status = "Delivered",
            CurrentLocation = "Customer Address",
            EventTimestamp = deliveryTimestamp,
            EventData = "{}"
        };

        var command = new ProcessDeliveryWebhookCommand { Dto = dto };
        var handler = new ProcessDeliveryWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedDelivery = await _context.Deliveries
            .FirstOrDefaultAsync(d => d.Id == delivery.Id);

        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Status.Should().Be(DeliveryStatus.Delivered);
        updatedDelivery.ActualDeliveryDate.Should().NotBeNull();
        updatedDelivery.ActualDeliveryDate.Should().BeCloseTo(deliveryTimestamp, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ProcessWebhook_InvalidStatusTransition_ShouldFail()
    {
        // Arrange
        var outbound = TestDataGenerator.GenerateOutbound();
        var delivery = TestDataGenerator.GenerateDelivery(outbound.Id, "1Z999AA10123456784");
        delivery.Status = DeliveryStatus.Delivered; // Already delivered

        _context.Outbounds.Add(outbound);
        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

        var dto = new DeliveryWebhookDto
        {
            TrackingNumber = "1Z999AA10123456784",
            PartnerEventId = "fedex_evt_invalid",
            Status = "Pending", // Invalid transition: Delivered -> Pending
            CurrentLocation = "Somewhere",
            EventData = "{}"
        };

        var command = new ProcessDeliveryWebhookCommand { Dto = dto };
        var handler = new ProcessDeliveryWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid status transition"));

        var updatedDelivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == delivery.Id);

        var invalidEvent = updatedDelivery!.DeliveryEvents
            .FirstOrDefault(e => e.EventType == "WebhookInvalidTransition");

        invalidEvent.Should().NotBeNull();
        invalidEvent!.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessWebhook_WithUnknownTracking_ShouldReturnError()
    {
        // Arrange
        var dto = new DeliveryWebhookDto
        {
            TrackingNumber = "UNKNOWN123456",
            PartnerEventId = "fedex_evt_001",
            Status = "InTransit",
            CurrentLocation = "Somewhere",
            EventData = "{}"
        };

        var command = new ProcessDeliveryWebhookCommand { Dto = dto };
        var handler = new ProcessDeliveryWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
