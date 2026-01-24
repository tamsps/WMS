using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.AddDeliveryEvent;

public class AddDeliveryEventCommandHandler : IRequestHandler<AddDeliveryEventCommand, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public AddDeliveryEventCommandHandler(
        WMSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> Handle(AddDeliveryEventCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Dto.DeliveryId, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        // Add event
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = request.Dto.EventType,
            EventDate = DateTime.UtcNow,
            Location = request.Dto.Location,
            Notes = request.Dto.Notes ?? request.Dto.EventDescription,
            CreatedBy = request.CurrentUser
        });

        delivery.UpdatedBy = request.CurrentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeliveryDto>.Success(
            DeliveryMapper.MapToDto(delivery),
            "Delivery event added successfully");
    }
}
