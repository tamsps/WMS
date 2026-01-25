using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.PutAwayInbound;

/// <summary>
/// Handler for marking inbound as put away
/// Indicates that goods have been moved to their designated storage locations
/// </summary>
public class PutAwayInboundCommandHandler : IRequestHandler<PutAwayInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PutAwayInboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(PutAwayInboundCommand request, CancellationToken cancellationToken)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Received)
        {
            return Result<InboundDto>.Failure(
                $"Cannot put away inbound in {inbound.Status} status. " +
                "Inbound must be in Received status.");
        }

        inbound.Status = InboundStatus.PutAway;
        inbound.UpdatedBy = request.CurrentUser;
        inbound.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InboundDto>.Success(
            InboundMapper.MapToDto(inbound),
            "Inbound put away successfully");
    }
}
