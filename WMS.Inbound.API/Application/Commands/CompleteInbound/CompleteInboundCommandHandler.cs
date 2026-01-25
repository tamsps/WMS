using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CompleteInbound;

/// <summary>
/// Handler for completing an inbound shipment
/// Marks the inbound process as completed after goods are put away
/// </summary>
public class CompleteInboundCommandHandler : IRequestHandler<CompleteInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteInboundCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(CompleteInboundCommand request, CancellationToken cancellationToken)
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

        // Can only complete from PutAway or Received status
        if (inbound.Status != InboundStatus.PutAway && inbound.Status != InboundStatus.Received)
        {
            return Result<InboundDto>.Failure(
                $"Cannot complete inbound in {inbound.Status} status. " +
                "Inbound must be in Received or PutAway status.");
        }

        inbound.Status = InboundStatus.Completed;
        inbound.UpdatedBy = request.CurrentUser;
        inbound.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InboundDto>.Success(
            InboundMapper.MapToDto(inbound),
            "Inbound completed successfully");
    }
}
