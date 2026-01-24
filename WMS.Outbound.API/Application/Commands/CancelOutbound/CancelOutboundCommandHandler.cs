using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.CancelOutbound;

public class CancelOutboundCommandHandler : IRequestHandler<CancelOutboundCommand, Result<OutboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOutboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OutboundDto>> Handle(CancelOutboundCommand request, CancellationToken cancellationToken)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .FirstOrDefaultAsync(o => o.Id == request.OutboundId, cancellationToken);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        if (outbound.Status == OutboundStatus.Shipped)
        {
            return Result<OutboundDto>.Failure("Cannot cancel a shipped outbound");
        }

        // Release any reserved inventory
        if (outbound.Status == OutboundStatus.Picked || outbound.Status == OutboundStatus.Packed)
        {
            foreach (var item in outbound.OutboundItems.Where(i => i.PickedQuantity > 0))
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.LocationId == item.LocationId, cancellationToken);

                if (inventory != null)
                {
                    inventory.QuantityReserved -= item.PickedQuantity;
                    inventory.UpdatedBy = request.CurrentUser;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        outbound.Status = OutboundStatus.Cancelled;
        outbound.UpdatedBy = request.CurrentUser;
        outbound.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OutboundDto>.Success(
            OutboundMapper.MapToDto(outbound),
            "Outbound cancelled successfully");
    }
}
