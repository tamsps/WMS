using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Repositories;
using WMS.Payment.API.Application.Commands.ProcessWebhook;
using WMS.Payment.API.DTOs.Payment;
using WMS.Tests.Fixtures;
using WMS.Tests.Helpers;

namespace WMS.Tests.Payment;

/// <summary>
/// Integration tests for Payment Webhook processing
/// Tests idempotency, duplicate detection, and audit logging
/// </summary>
public class PaymentWebhookTests : IDisposable
{
    private readonly WMSDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<ILogger<ProcessWebhookCommandHandler>> _loggerMock;

    public PaymentWebhookTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _unitOfWork = new UnitOfWork(_context);
        _loggerMock = new Mock<ILogger<ProcessWebhookCommandHandler>>();
        TestDataGenerator.ResetCounters();
    }

    [Fact]
    public async Task ProcessWebhook_FirstTime_ShouldUpdatePaymentStatus()
    {
        // Arrange
        var payment = TestDataGenerator.GeneratePayment();
        payment.ExternalPaymentId = "pay_test123";
        payment.Status = PaymentStatus.Pending;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var dto = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_unique_001",
            Status = "Confirmed",
            EventData = "{\"amount\": 1000}",
            GatewayTimestamp = DateTime.UtcNow
        };

        var command = new ProcessWebhookCommand { Dto = dto };
        var handler = new ProcessWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedPayment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == "pay_test123");

        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentStatus.Confirmed);
        updatedPayment.PaymentEvents.Should().HaveCount(1);
        updatedPayment.PaymentEvents.First().EventType.Should().Be("WebhookReceived");
        updatedPayment.PaymentEvents.First().GatewayEventId.Should().Be("evt_unique_001");
        updatedPayment.PaymentEvents.First().IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhook_DuplicateEvent_ShouldIgnoreAndLogDuplicate()
    {
        // Arrange
        var payment = TestDataGenerator.GeneratePayment();
        payment.ExternalPaymentId = "pay_test123";
        payment.Status = PaymentStatus.Pending;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // First webhook
        var dto1 = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_duplicate_001",
            Status = "Confirmed",
            EventData = "{\"amount\": 1000}"
        };

        var command1 = new ProcessWebhookCommand { Dto = dto1 };
        var handler = new ProcessWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);
        await handler.Handle(command1, CancellationToken.None);

        // Second webhook (duplicate)
        var dto2 = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_duplicate_001", // Same event ID
            Status = "Confirmed",
            EventData = "{\"amount\": 1000}"
        };

        var command2 = new ProcessWebhookCommand { Dto = dto2 };

        // Act
        var result = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("already processed");

        var updatedPayment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == "pay_test123");

        updatedPayment.Should().NotBeNull();
        updatedPayment!.PaymentEvents.Should().HaveCount(2);

        var duplicateEvent = updatedPayment.PaymentEvents
            .FirstOrDefault(e => e.EventType == "WebhookDuplicate");

        duplicateEvent.Should().NotBeNull();
        duplicateEvent!.IsProcessed.Should().BeFalse();
        duplicateEvent.GatewayEventId.Should().Be("evt_duplicate_001");
    }

    [Fact]
    public async Task ProcessWebhook_WithInvalidStatus_ShouldLogError()
    {
        // Arrange
        var payment = TestDataGenerator.GeneratePayment();
        payment.ExternalPaymentId = "pay_test123";

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var dto = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_invalid_001",
            Status = "InvalidStatus",  // Invalid status
            EventData = "{}"
        };

        var command = new ProcessWebhookCommand { Dto = dto };
        var handler = new ProcessWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        var updatedPayment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == "pay_test123");

        var invalidEvent = updatedPayment!.PaymentEvents
            .FirstOrDefault(e => e.EventType == "WebhookInvalidStatus");

        invalidEvent.Should().NotBeNull();
        invalidEvent!.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessWebhook_WithUnknownPayment_ShouldReturnError()
    {
        // Arrange
        var dto = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_unknown",
            GatewayEventId = "evt_001",
            Status = "Confirmed",
            EventData = "{}"
        };

        var command = new ProcessWebhookCommand { Dto = dto };
        var handler = new ProcessWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    [Fact]
    public async Task ProcessWebhook_MultipleDifferentEvents_ShouldProcessAll()
    {
        // Arrange
        var payment = TestDataGenerator.GeneratePayment();
        payment.ExternalPaymentId = "pay_test123";
        payment.Status = PaymentStatus.Pending;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var handler = new ProcessWebhookCommandHandler(_context, _unitOfWork, _loggerMock.Object);

        // Event 1: Confirmed
        var dto1 = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_001",
            Status = "Confirmed",
            EventData = "{}"
        };
        var command1 = new ProcessWebhookCommand { Dto = dto1 };
        var result1 = await handler.Handle(command1, CancellationToken.None);

        // Event 2: Different event (should still process)
        var dto2 = new PaymentWebhookDto
        {
            ExternalPaymentId = "pay_test123",
            GatewayEventId = "evt_002",  // Different event ID
            Status = "Confirmed",
            EventData = "{}"
        };
        var command2 = new ProcessWebhookCommand { Dto = dto2 };
        var result2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        var updatedPayment = await _context.Payments
            .Include(p => p.PaymentEvents)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == "pay_test123");

        updatedPayment!.PaymentEvents.Should().HaveCount(2);
        updatedPayment.PaymentEvents.All(e => e.EventType == "WebhookReceived").Should().BeTrue();
        updatedPayment.PaymentEvents.All(e => e.IsProcessed).Should().BeTrue();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
