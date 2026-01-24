using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.UpdateDeliveryStatus;

public class UpdateDeliveryStatusCommandHandler : IRequestHandler<UpdateDeliveryStatusCommand, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeliveryStatusCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> Handle(UpdateDeliveryStatusCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Dto.DeliveryId, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        // Parse and validate status
        if (!Enum.TryParse<DeliveryStatus>(request.Dto.Status, out var newStatus))
        {
            return Result<DeliveryDto>.Failure($"Invalid delivery status: {request.Dto.Status}");
        }

        // Validate status transition
        if (!IsValidStatusTransition(delivery.Status, newStatus))
        {
            return Result<DeliveryDto>.Failure($"Cannot change status from {delivery.Status} to {newStatus}");
        }

        delivery.Status = newStatus;
        delivery.UpdatedBy = request.CurrentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        // Set dates based on status
        if (newStatus == DeliveryStatus.InTransit && !delivery.PickupDate.HasValue)
        {
            delivery.PickupDate = DateTime.UtcNow;
        }

        // Add event
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = newStatus.ToString(),
            EventDate = DateTime.UtcNow,
            Location = request.Dto.EventLocation,
            Notes = request.Dto.Notes,
            CreatedBy = request.CurrentUser
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeliveryDto>.Success(
            DeliveryMapper.MapToDto(delivery),
            "Delivery status updated successfully");
    }

    private bool IsValidStatusTransition(DeliveryStatus current, DeliveryStatus next)
    {
        return (current, next) switch
        {
            (DeliveryStatus.Pending, DeliveryStatus.InTransit) => true,
            (DeliveryStatus.InTransit, DeliveryStatus.Delivered) => true,
            (DeliveryStatus.InTransit, DeliveryStatus.Failed) => true,
            (DeliveryStatus.Pending, DeliveryStatus.Cancelled) => true,
            _ => false
        };
    }
}
