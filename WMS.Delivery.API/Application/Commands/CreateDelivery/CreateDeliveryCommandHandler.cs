using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Commands.CreateDelivery;

public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<WMS.Domain.Entities.Delivery> _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDeliveryCommandHandler(
        WMSDbContext context,
        IRepository<WMS.Domain.Entities.Delivery> deliveryRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
    {
        // Validate outbound exists
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OutboundId, cancellationToken);

        if (outbound == null)
        {
            return Result<DeliveryDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Shipped && outbound.Status != OutboundStatus.Picked)
        {
            return Result<DeliveryDto>.Failure($"Cannot create delivery for outbound in {outbound.Status} status");
        }

        // Check if delivery already exists for this outbound
        var existingDelivery = await _context.Deliveries
            .FirstOrDefaultAsync(d => d.OutboundId == request.Dto.OutboundId, cancellationToken);

        if (existingDelivery != null)
        {
            return Result<DeliveryDto>.Failure("Delivery already exists for this outbound");
        }

        var delivery = new WMS.Domain.Entities.Delivery
        {
            DeliveryNumber = await GenerateDeliveryNumberAsync(cancellationToken),
            OutboundId = request.Dto.OutboundId,
            Status = DeliveryStatus.Pending,
            ShippingAddress = request.Dto.ShippingAddress,
            ShippingCity = request.Dto.ShippingCity,
            ShippingState = request.Dto.ShippingState,
            ShippingZipCode = request.Dto.ShippingZipCode,
            ShippingCountry = request.Dto.ShippingCountry,
            Carrier = request.Dto.Carrier,
            TrackingNumber = request.Dto.TrackingNumber,
            VehicleNumber = request.Dto.VehicleNumber,
            DriverName = request.Dto.DriverName,
            DriverPhone = request.Dto.DriverPhone,
            EstimatedDeliveryDate = request.Dto.EstimatedDeliveryDate,
            CreatedBy = request.CurrentUser
        };

        // Add initial event
        delivery.DeliveryEvents.Add(new WMS.Domain.Entities.DeliveryEvent
        {
            EventType = "Created",
            EventDate = DateTime.UtcNow,
            Notes = "Delivery created",
            CreatedBy = request.CurrentUser
        });

        await _deliveryRepository.AddAsync(delivery, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with includes
        var created = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == delivery.Id, cancellationToken);

        return Result<DeliveryDto>.Success(
            DeliveryMapper.MapToDto(created!),
            "Delivery created successfully");
    }

    private async Task<string> GenerateDeliveryNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"DEL-{today:yyyyMMdd}";

        var lastDelivery = await _context.Deliveries
            .Where(d => d.DeliveryNumber.StartsWith(prefix))
            .OrderByDescending(d => d.DeliveryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastDelivery == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastDelivery.DeliveryNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
