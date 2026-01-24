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
                    // Create or update inventory record directly
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
                        Notes = $"Received from inbound {inbound.InboundNumber}",
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
