using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.ReceiveInbound;

/// <summary>
/// Handler for receiving inbound shipment items
/// 
/// ATOMIC TRANSACTION PROCESSING:
/// This operation must execute as an atomic transaction. All or nothing:
/// 1. Pre-validates all items (capacity, quantities, status)
/// 2. Begins database transaction (SERIALIZABLE isolation level)
/// 3. Updates inbound item receive quantities
/// 4. Validates and updates location capacity
/// 5. Creates or updates inventory records (with concurrency check)
/// 6. Creates inventory transaction audit trail
/// 7. Updates inbound status to Received
/// 8. Commits transaction OR rolls back on any failure
/// 
/// BUSINESS RULES ENFORCED:
/// - Only Pending inbounds can be received
/// - Location capacity must be sufficient before receiving
/// - Product dimensions × quantity = capacity required
/// - Damaged quantities do not increase inventory
/// - Good quantity (received - damaged) increases inventory
/// - All changes are tracked in inventory transactions
/// 
/// CONCURRENCY CONTROL:
/// - Uses optimistic concurrency (RowVersion) on Inventory entity
/// - DbUpdateConcurrencyException caught and returned as user-friendly error
/// - User must refresh and retry if inventory changed by another operation
/// - Prevents: overselling, negative stock, lost updates
/// </summary>
public class ReceiveInboundCommandHandler : IRequestHandler<ReceiveInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ReceiveInboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(ReceiveInboundCommand request, CancellationToken cancellationToken)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == request.Dto.InboundId, cancellationToken);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Pending)
        {
            return Result<InboundDto>.Failure($"Cannot receive inbound in {inbound.Status} status");
        }

        // Pre-validate capacity for all items before making any changes
        foreach (var receiveItem in request.Dto.Items)
        {
            var inboundItem = inbound.InboundItems.FirstOrDefault(ii => ii.Id == receiveItem.InboundItemId);
            if (inboundItem == null)
            {
                return Result<InboundDto>.Failure($"Inbound item {receiveItem.InboundItemId} not found");
            }

            var goodQuantity = receiveItem.ReceivedQuantity - (receiveItem.DamagedQuantity ?? 0);
            if (goodQuantity > 0)
            {
                // Calculate required capacity based on product dimensions and quantity
                var product = inboundItem.Product;
                var requiredCapacity = (product.Length * product.Width * product.Height / 1000000) * goodQuantity; // Convert cm³ to m³

                // Check location capacity
                var location = inboundItem.Location;
                var availableCapacity = location.Capacity - location.CurrentOccupancy;

                if (requiredCapacity > availableCapacity)
                {
                    return Result<InboundDto>.Failure(
                        $"Insufficient capacity at location {location.Code}. " +
                        $"Required: {requiredCapacity:F2} m³, Available: {availableCapacity:F2} m³, " +
                        $"Shortage: {(requiredCapacity - availableCapacity):F2} m³");
                }
            }
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var receiveItem in request.Dto.Items)
            {
                var inboundItem = inbound.InboundItems.FirstOrDefault(ii => ii.Id == receiveItem.InboundItemId);
                if (inboundItem == null)
                {
                    return Result<InboundDto>.Failure($"Inbound item {receiveItem.InboundItemId} not found");
                }

                inboundItem.ReceivedQuantity = receiveItem.ReceivedQuantity;
                inboundItem.DamagedQuantity = receiveItem.DamagedQuantity;
                if (!string.IsNullOrWhiteSpace(receiveItem.Notes))
                {
                    inboundItem.Notes = receiveItem.Notes;
                }
                inboundItem.UpdatedBy = request.CurrentUser;
                inboundItem.UpdatedAt = DateTime.UtcNow;

                // Update inventory for received quantity (excluding damaged)
                var goodQuantity = receiveItem.ReceivedQuantity - (receiveItem.DamagedQuantity ?? 0);
                if (goodQuantity > 0)
                {
                    // Calculate capacity used
                    var product = inboundItem.Product;
                    var capacityUsed = (product.Length * product.Width * product.Height / 1000000) * goodQuantity;

                    // Update location occupancy
                    var location = await _context.Locations.FindAsync(new object[] { inboundItem.LocationId }, cancellationToken);
                    if (location != null)
                    {
                        location.CurrentOccupancy += capacityUsed;
                        location.UpdatedBy = request.CurrentUser;
                        location.UpdatedAt = DateTime.UtcNow;
                    }

                    // Create or update inventory record
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv => inv.ProductId == inboundItem.ProductId && 
                                                   inv.LocationId == inboundItem.LocationId, 
                                          cancellationToken);

                    decimal balanceBefore;
                    if (inventory == null)
                    {
                        balanceBefore = 0;
                        // Create new inventory record
                        inventory = new Inventory
                        {
                            ProductId = inboundItem.ProductId,
                            LocationId = inboundItem.LocationId,
                            QuantityOnHand = goodQuantity,
                            LastStockDate = DateTime.UtcNow,
                            CreatedBy = request.CurrentUser,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Inventories.Add(inventory);
                    }
                    else
                    {
                        balanceBefore = inventory.QuantityOnHand;
                        // Update existing inventory
                        inventory.QuantityOnHand += goodQuantity;
                        inventory.LastStockDate = DateTime.UtcNow;
                        inventory.UpdatedBy = request.CurrentUser;
                        inventory.UpdatedAt = DateTime.UtcNow;
                    }

                    // Create inventory transaction record
                    var transaction = new InventoryTransaction
                    {
                        TransactionNumber = await GenerateTransactionNumberAsync(cancellationToken),
                        TransactionType = TransactionType.Inbound,
                        ProductId = inboundItem.ProductId,
                        LocationId = inboundItem.LocationId,
                        Quantity = goodQuantity,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore + goodQuantity,
                        ReferenceId = inbound.Id,
                        ReferenceType = "Inbound",
                        ReferenceNumber = inbound.InboundNumber,
                        Notes = $"Received from inbound {inbound.InboundNumber}. Capacity used: {capacityUsed:F4} m³",
                        CreatedBy = request.CurrentUser,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.InventoryTransactions.Add(transaction);
                }
            }

            inbound.Status = InboundStatus.Received;
            inbound.ReceivedDate = DateTime.UtcNow;
            inbound.UpdatedBy = request.CurrentUser;
            inbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<InboundDto>.Success(InboundMapper.MapToDto(inbound), "Inbound received successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InboundDto>.Failure(
                "Inventory was modified by another user. Please refresh and try again. " +
                "This ensures accurate inventory levels and prevents overselling.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InboundDto>.Failure($"Failed to receive inbound: {ex.Message}");
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
