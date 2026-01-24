using MediatR;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Common.Models;

namespace WMS.Inbound.API.Application.Commands.CancelInbound;

public class CancelInboundCommandHandler : IRequestHandler<CancelInboundCommand, Result>
{
    private readonly IRepository<WMS.Domain.Entities.Inbound> _inboundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelInboundCommandHandler(
        IRepository<WMS.Domain.Entities.Inbound> inboundRepository,
        IUnitOfWork unitOfWork)
    {
        _inboundRepository = inboundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelInboundCommand request, CancellationToken cancellationToken)
    {
        var inbound = await _inboundRepository.GetByIdAsync(request.Id, cancellationToken);
        if (inbound == null)
        {
            return Result.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Pending)
        {
            return Result.Failure($"Cannot cancel inbound in {inbound.Status} status");
        }

        inbound.Status = InboundStatus.Cancelled;
        inbound.UpdatedBy = request.CurrentUser;
        inbound.UpdatedAt = DateTime.UtcNow;

        await _inboundRepository.UpdateAsync(inbound);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Inbound cancelled successfully");
    }
}
