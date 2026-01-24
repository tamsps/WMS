using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.CompleteDelivery;

public class CompleteDeliveryCommandHandler : IRequestHandler<CompleteDeliveryCommand, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteDeliveryCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Dto.DeliveryId, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (delivery.Status != DeliveryStatus.InTransit)
        {
            return Result<DeliveryDto>.Failure($"Cannot complete delivery in {delivery.Status} status");
        }

        delivery.Status = DeliveryStatus.Delivered;
        delivery.ActualDeliveryDate = DateTime.UtcNow;
        delivery.UpdatedBy = request.CurrentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        // Add completion event
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "Delivered",
            EventDate = DateTime.UtcNow,
            Notes = request.Dto.Notes ?? "Delivery completed successfully",
            CreatedBy = request.CurrentUser
        });

        // Update outbound status to Shipped (since delivery completed doesn't mean it was delivered to outbound)
        // Outbound was already shipped when delivery was created
        if (delivery.Outbound != null)
        {
            // Keep outbound status as Shipped - no "Delivered" status for outbound
            delivery.Outbound.UpdatedBy = request.CurrentUser;
            delivery.Outbound.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeliveryDto>.Success(
            DeliveryMapper.MapToDto(delivery),
            "Delivery completed successfully");
    }
}
