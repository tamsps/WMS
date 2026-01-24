using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.FailDelivery;

public class FailDeliveryCommandHandler : IRequestHandler<FailDeliveryCommand, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public FailDeliveryCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> Handle(FailDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Dto.DeliveryId, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (delivery.Status == DeliveryStatus.Delivered)
        {
            return Result<DeliveryDto>.Failure("Cannot fail a delivered shipment");
        }

        delivery.Status = DeliveryStatus.Failed;
        delivery.FailureReason = request.Dto.FailureReason;
        delivery.UpdatedBy = request.CurrentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        // Add failure event
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "Failed",
            EventDate = DateTime.UtcNow,
            Notes = request.Dto.FailureReason,
            CreatedBy = request.CurrentUser
        });

        // Keep outbound status as is - delivery failure doesn't change outbound status
        // The outbound was already shipped
        if (delivery.Outbound != null)
        {
            delivery.Outbound.UpdatedBy = request.CurrentUser;
            delivery.Outbound.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeliveryDto>.Success(
            DeliveryMapper.MapToDto(delivery),
            "Delivery marked as failed");
    }
}
