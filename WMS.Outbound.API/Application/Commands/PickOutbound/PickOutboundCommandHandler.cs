using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.PickOutbound;

/// <summary>
/// Handler for picking outbound items
/// 
/// INVENTORY RESERVATION PROCESS:
/// - Validates available quantity before reserving
/// - Increases QuantityReserved atomically
/// - Creates InventoryTransaction with TransactionType.Reserve
/// - Uses optimistic concurrency control (RowVersion)
/// - Prevents overselling through Available = OnHand - Reserved
/// 
/// CONCURRENCY CONTROL:
/// - Multiple pickers can work simultaneously
/// - Optimistic concurrency prevents double-allocation
/// - User notified if inventory changed during pick
/// - Must refresh and verify availability before retry
/// </summary>
public class PickOutboundCommandHandler : IRequestHandler<PickOutboundCommand, Result<OutboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PickOutboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OutboundDto>> Handle(PickOutboundCommand request, CancellationToken cancellationToken)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OutboundId, cancellationToken);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Pending && outbound.Status != OutboundStatus.Picking)
        {
            return Result<OutboundDto>.Failure($"Cannot pick outbound in {outbound.Status} status");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update picked quantities for items
            foreach (var itemDto in request.Dto.Items)
            {
                var item = outbound.OutboundItems.FirstOrDefault(oi => oi.Id == itemDto.OutboundItemId);
                if (item == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<OutboundDto>.Failure($"Outbound item {itemDto.OutboundItemId} not found");
                }

                if (itemDto.PickedQuantity > item.OrderedQuantity)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<OutboundDto>.Failure($"Picked quantity cannot exceed ordered quantity for item {item.Product.SKU}");
                }

                // Reserve inventory
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.LocationId == item.LocationId, cancellationToken);

                if (inventory == null || inventory.QuantityAvailable < itemDto.PickedQuantity)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<OutboundDto>.Failure(
                        $"Insufficient available inventory for product {item.Product.SKU}. " +
                        $"Available: {inventory?.QuantityAvailable ?? 0}, Required: {itemDto.PickedQuantity}");
                }

                // Record balance before reservation
                decimal reservedBefore = inventory.QuantityReserved;

                // Update inventory reservation
                inventory.QuantityReserved += itemDto.PickedQuantity;
                inventory.LastStockDate = DateTime.UtcNow;
                inventory.UpdatedBy = request.CurrentUser;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Create inventory transaction for reservation
                var transaction = new InventoryTransaction
                {
                    TransactionNumber = await GenerateTransactionNumberAsync(cancellationToken),
                    TransactionType = TransactionType.Reserve,
                    ProductId = item.ProductId,
                    LocationId = item.LocationId,
                    Quantity = itemDto.PickedQuantity,
                    BalanceBefore = inventory.QuantityOnHand, // On-hand doesn't change, but we record for reference
                    BalanceAfter = inventory.QuantityOnHand,  // Still same on-hand
                    ReferenceId = outbound.Id,
                    ReferenceType = "OutboundPick",
                    ReferenceNumber = outbound.OutboundNumber,
                    Notes = $"Reserved {itemDto.PickedQuantity} units for outbound {outbound.OutboundNumber}. Reserved: {reservedBefore} -> {inventory.QuantityReserved}",
                    CreatedBy = request.CurrentUser,
                    CreatedAt = DateTime.UtcNow
                };
                _context.InventoryTransactions.Add(transaction);
                
                item.PickedQuantity = itemDto.PickedQuantity;
                item.UpdatedBy = request.CurrentUser;
                item.UpdatedAt = DateTime.UtcNow;
            }

            outbound.Status = OutboundStatus.Picked;
            outbound.UpdatedBy = request.CurrentUser;
            outbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<OutboundDto>.Success(
                OutboundMapper.MapToDto(outbound),
                "Outbound picking completed successfully. Inventory reserved.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<OutboundDto>.Failure(
                "Inventory was modified by another user during picking. " +
                "Another pick or receipt may have occurred. " +
                "Please refresh and verify inventory availability before retrying.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<OutboundDto>.Failure($"Failed to pick outbound: {ex.Message}");
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
