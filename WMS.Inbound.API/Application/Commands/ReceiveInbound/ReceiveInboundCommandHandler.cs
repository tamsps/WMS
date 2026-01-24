using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
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
    private readonly WMS.Application.Interfaces.IInventoryService _inventoryService;

    public ReceiveInboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork,
        WMS.Application.Interfaces.IInventoryService inventoryService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
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
                    var inventoryResult = await _inventoryService.UpdateInventoryAsync(
                        inboundItem.ProductId,
                        inboundItem.LocationId,
                        goodQuantity,
                        TransactionType.Inbound,
                        inbound.InboundNumber,
                        inbound.Id,
                        request.CurrentUser
                    );

                    if (!inventoryResult.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Result<InboundDto>.Failure(inventoryResult.Errors.FirstOrDefault() ?? "Failed to update inventory");
                    }
                }
            }

            inbound.Status = InboundStatus.Received;
            inbound.ReceivedDate = DateTime.UtcNow;
            inbound.UpdatedBy = request.CurrentUser;
            inbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<InboundDto>.Success(InboundMapper.MapToDto(inbound), "Inbound received successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<InboundDto>.Failure($"Failed to receive inbound: {ex.Message}");
        }
    }
}
