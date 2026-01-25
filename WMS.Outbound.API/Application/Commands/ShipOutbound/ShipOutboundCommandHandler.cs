using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.ShipOutbound;

/// <summary>
/// Handler for shipping outbound orders
/// 
/// BUSINESS RULES ENFORCED:
/// - Payment validation for prepaid orders (must be confirmed before shipping)
/// - Inventory deduction (atomic transaction)
/// - Only Picked or Packed outbounds can be shipped
/// - COD and Postpaid orders can ship without payment confirmation
/// - Creates inventory transaction audit trail
/// </summary>
public class ShipOutboundCommandHandler : IRequestHandler<ShipOutboundCommand, Result<OutboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShipOutboundCommandHandler(
        WMSDbContext context,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OutboundDto>> Handle(ShipOutboundCommand request, CancellationToken cancellationToken)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .Include(o => o.Payment)  // Include payment for validation
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OutboundId, cancellationToken);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Picked && outbound.Status != OutboundStatus.Packed)
        {
            return Result<OutboundDto>.Failure($"Cannot ship outbound in {outbound.Status} status");
        }

        // PAYMENT VALIDATION: For prepaid orders, payment must be confirmed before shipping
        if (outbound.PaymentId.HasValue && outbound.Payment != null)
        {
            if (outbound.Payment.PaymentType == PaymentType.Prepaid)
            {
                if (outbound.Payment.Status != PaymentStatus.Confirmed)
                {
                    return Result<OutboundDto>.Failure(
                        "Cannot ship prepaid order. Payment must be confirmed before shipping. " +
                        $"Current payment status: {outbound.Payment.Status}");
                }
            }
            // COD and Postpaid orders can proceed without payment confirmation
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Process each item - deduct inventory
            foreach (var item in outbound.OutboundItems)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.LocationId == item.LocationId, cancellationToken);

                if (inventory == null)
                {
                    return Result<OutboundDto>.Failure($"Inventory not found for product {item.Product.SKU}");
                }

                var shippedQuantity = item.PickedQuantity; // Ship what was picked

                // Validate sufficient quantity
                if (inventory.QuantityReserved < shippedQuantity)
                {
                    return Result<OutboundDto>.Failure(
                        $"Insufficient reserved quantity for product {item.Product.SKU}. " +
                        $"Required: {shippedQuantity}, Reserved: {inventory.QuantityReserved}");
                }

                // Deduct from reserved and on-hand
                inventory.QuantityReserved -= shippedQuantity;
                inventory.QuantityOnHand -= shippedQuantity;
                inventory.LastStockDate = DateTime.UtcNow;
                inventory.UpdatedBy = request.CurrentUser;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Update location occupancy (reduce capacity usage)
                var location = await _context.Locations.FindAsync(new object[] { item.LocationId }, cancellationToken);
                if (location != null)
                {
                    var product = item.Product;
                    var capacityReleased = (product.Length * product.Width * product.Height / 1000000) * shippedQuantity;
                    location.CurrentOccupancy -= capacityReleased;
                    if (location.CurrentOccupancy < 0) location.CurrentOccupancy = 0; // Prevent negative
                    location.UpdatedBy = request.CurrentUser;
                    location.UpdatedAt = DateTime.UtcNow;
                }

                // Create inventory transaction
                var transaction = new InventoryTransaction
                {
                    TransactionNumber = await GenerateTransactionNumberAsync(cancellationToken),
                    TransactionType = TransactionType.Outbound,
                    ProductId = item.ProductId,
                    LocationId = item.LocationId,
                    Quantity = -shippedQuantity, // Negative for outbound
                    BalanceBefore = inventory.QuantityOnHand + shippedQuantity,
                    BalanceAfter = inventory.QuantityOnHand,
                    ReferenceId = outbound.Id,
                    ReferenceType = "Outbound",
                    ReferenceNumber = outbound.OutboundNumber,
                    Notes = $"Shipped from outbound {outbound.OutboundNumber}",
                    CreatedBy = request.CurrentUser
                };

                await _transactionRepository.AddAsync(transaction, cancellationToken);

                item.ShippedQuantity = shippedQuantity;
                item.UpdatedBy = request.CurrentUser;
                item.UpdatedAt = DateTime.UtcNow;
            }

            outbound.Status = OutboundStatus.Shipped;
            outbound.ShipDate = DateTime.UtcNow;
            outbound.UpdatedBy = request.CurrentUser;
            outbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<OutboundDto>.Success(
                OutboundMapper.MapToDto(outbound),
                "Outbound shipped successfully. Inventory updated and capacity released.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<OutboundDto>.Failure(
                "Inventory was modified by another user during shipping. " +
                "This could be due to concurrent picking, receiving, or adjustments. " +
                "Please refresh the page and verify inventory levels before retrying.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<OutboundDto>.Failure($"Failed to ship outbound: {ex.Message}");
        }
    }

    private async Task<string> GenerateTransactionNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"TXN-{today:yyyyMMdd}";

        var lastTransaction = await _context.InventoryTransactions
            .Where(t => t.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastTransaction == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastTransaction.TransactionNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
