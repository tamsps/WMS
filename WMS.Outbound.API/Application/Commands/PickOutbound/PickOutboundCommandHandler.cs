using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.PickOutbound;

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

        // Update picked quantities for items
        foreach (var itemDto in request.Dto.Items)
        {
            var item = outbound.OutboundItems.FirstOrDefault(oi => oi.Id == itemDto.OutboundItemId);
            if (item == null)
            {
                return Result<OutboundDto>.Failure($"Outbound item {itemDto.OutboundItemId} not found");
            }

            if (itemDto.PickedQuantity > item.OrderedQuantity)
            {
                return Result<OutboundDto>.Failure($"Picked quantity cannot exceed ordered quantity for item {item.Product.SKU}");
            }

            // Reserve inventory
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.LocationId == item.LocationId, cancellationToken);

            if (inventory == null || inventory.QuantityAvailable < itemDto.PickedQuantity)
            {
                return Result<OutboundDto>.Failure($"Insufficient available inventory for product {item.Product.SKU}");
            }

            // Update inventory reservation
            inventory.QuantityReserved += itemDto.PickedQuantity;
            item.PickedQuantity = itemDto.PickedQuantity;
            item.UpdatedBy = request.CurrentUser;
            item.UpdatedAt = DateTime.UtcNow;
        }

        outbound.Status = OutboundStatus.Picked;
        outbound.UpdatedBy = request.CurrentUser;
        outbound.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OutboundDto>.Success(
            OutboundMapper.MapToDto(outbound),
            "Outbound picking completed successfully");
    }
}
