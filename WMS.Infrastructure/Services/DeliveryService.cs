using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Delivery;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class DeliveryService : IDeliveryService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Delivery> _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeliveryService(
        WMSDbContext context,
        IRepository<Delivery> deliveryRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeliveryDto>> GetByIdAsync(Guid id)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents.OrderBy(e => e.CreatedAt))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        return Result<DeliveryDto>.Success(MapToDto(delivery));
    }

    public async Task<Result<DeliveryDto>> GetByOutboundIdAsync(Guid outboundId)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents.OrderBy(e => e.CreatedAt))
            .FirstOrDefaultAsync(d => d.OutboundId == outboundId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found for this outbound");
        }

        return Result<DeliveryDto>.Success(MapToDto(delivery));
    }

    public async Task<Result<PagedResult<DeliveryDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null)
    {
        var query = _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents.OrderBy(e => e.CreatedAt))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DeliveryStatus>(status, out var deliveryStatus))
        {
            query = query.Where(d => d.Status == deliveryStatus);
        }

        var totalCount = await query.CountAsync();
        var deliveries = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<DeliveryDto>
        {
            Items = deliveries.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<DeliveryDto>>.Success(result);
    }

    public async Task<Result<DeliveryDto>> CreateAsync(CreateDeliveryDto dto, string currentUser)
    {
        // Validate outbound exists
        var outbound = await _context.Outbounds.FindAsync(dto.OutboundId);
        if (outbound == null)
        {
            return Result<DeliveryDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Shipped)
        {
            return Result<DeliveryDto>.Failure("Can only create delivery for shipped outbounds");
        }

        // Check if delivery already exists for this outbound
        var existingDelivery = await _context.Deliveries
            .FirstOrDefaultAsync(d => d.OutboundId == dto.OutboundId);
        
        if (existingDelivery != null)
        {
            return Result<DeliveryDto>.Failure("Delivery already exists for this outbound");
        }

        var delivery = new Delivery
        {
            OutboundId = dto.OutboundId,
            DeliveryNumber = await GenerateDeliveryNumberAsync(),
            Status = DeliveryStatus.Pending,
            Carrier = dto.Carrier,
            TrackingNumber = dto.TrackingNumber,
            VehicleNumber = dto.VehicleNumber,
            DriverName = dto.DriverName,
            DriverPhone = dto.DriverPhone,
            EstimatedDeliveryDate = dto.EstimatedDeliveryDate,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingZipCode = dto.ShippingZipCode,
            ShippingCountry = dto.ShippingCountry,
            CreatedBy = currentUser
        };

        // Add initial delivery event
        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "Created",
            EventDate = DateTime.UtcNow,
            Notes = $"Delivery created with carrier {dto.Carrier}",
            CreatedBy = currentUser
        });

        await _deliveryRepository.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Delivery created successfully");
    }

    public async Task<Result<DeliveryDto>> UpdateStatusAsync(UpdateDeliveryStatusDto dto, string currentUser)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == dto.DeliveryId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (!Enum.TryParse<DeliveryStatus>(dto.Status, out var status))
        {
            return Result<DeliveryDto>.Failure("Invalid delivery status");
        }

        delivery.Status = status;
        delivery.UpdatedBy = currentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        if (status == DeliveryStatus.Delivered && !delivery.ActualDeliveryDate.HasValue)
        {
            delivery.ActualDeliveryDate = DateTime.UtcNow;
        }

        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "StatusUpdate",
            EventDate = DateTime.UtcNow,
            Location = dto.EventLocation,
            Notes = dto.Notes ?? $"Status changed to {status}",
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Delivery status updated successfully");
    }

    public async Task<Result<DeliveryDto>> AddEventAsync(AddDeliveryEventDto dto, string currentUser)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == dto.DeliveryId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = dto.EventType,
            EventDate = DateTime.UtcNow,
            Location = dto.Location,
            Notes = dto.EventDescription,
            CreatedBy = currentUser
        });

        delivery.UpdatedBy = currentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Delivery event added successfully");
    }

    public async Task<Result<DeliveryDto>> MarkAsDeliveredAsync(Guid deliveryId, string currentUser, string? recipientName = null)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (delivery.Status == DeliveryStatus.Delivered)
        {
            return Result<DeliveryDto>.Failure("Delivery is already marked as delivered");
        }

        if (delivery.Status == DeliveryStatus.Cancelled || delivery.Status == DeliveryStatus.Returned)
        {
            return Result<DeliveryDto>.Failure($"Cannot mark {delivery.Status} delivery as delivered");
        }

        delivery.Status = DeliveryStatus.Delivered;
        delivery.ActualDeliveryDate = DateTime.UtcNow;
        delivery.UpdatedBy = currentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "Delivered",
            EventDate = DateTime.UtcNow,
            Notes = $"Package delivered{(recipientName != null ? $" to {recipientName}" : "")}",
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Delivery marked as delivered successfully");
    }

    public async Task<Result<DeliveryDto>> InitiateReturnAsync(Guid deliveryId, string currentUser, string reason)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (delivery.Status != DeliveryStatus.Delivered && delivery.Status != DeliveryStatus.InTransit)
        {
            return Result<DeliveryDto>.Failure("Can only initiate return for delivered or in-transit deliveries");
        }

        delivery.Status = DeliveryStatus.Returned;
        delivery.UpdatedBy = currentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "ReturnInitiated",
            EventDate = DateTime.UtcNow,
            Notes = reason,
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Return initiated successfully");
    }

    public async Task<Result<DeliveryDto>> CompleteAsync(CompleteDeliveryDto dto, string currentUser)
    {
        return await MarkAsDeliveredAsync(dto.DeliveryId, currentUser, dto.Notes);
    }

    public async Task<Result<DeliveryDto>> FailAsync(FailDeliveryDto dto, string currentUser)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == dto.DeliveryId);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        if (delivery.Status == DeliveryStatus.Delivered)
        {
            return Result<DeliveryDto>.Failure("Cannot fail a delivered delivery");
        }

        delivery.Status = DeliveryStatus.Failed;
        delivery.UpdatedBy = currentUser;
        delivery.UpdatedAt = DateTime.UtcNow;

        delivery.DeliveryEvents.Add(new DeliveryEvent
        {
            EventType = "Failed",
            EventDate = DateTime.UtcNow,
            Notes = dto.FailureReason,
            CreatedBy = currentUser
        });

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(delivery.Id);
        return Result<DeliveryDto>.Success(result.Data!, "Delivery marked as failed");
    }

    public async Task<Result<DeliveryDto>> GetByTrackingNumberAsync(string trackingNumber)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents.OrderBy(e => e.CreatedAt))
            .FirstOrDefaultAsync(d => d.TrackingNumber == trackingNumber);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found with specified tracking number");
        }

        return Result<DeliveryDto>.Success(MapToDto(delivery));
    }

    private async Task<string> GenerateDeliveryNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"DEL-{today:yyyyMMdd}";
        
        var lastDelivery = await _context.Deliveries
            .Where(d => d.DeliveryNumber.StartsWith(prefix))
            .OrderByDescending(d => d.DeliveryNumber)
            .FirstOrDefaultAsync();

        if (lastDelivery == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastDelivery.DeliveryNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    private static DeliveryDto MapToDto(Delivery delivery)
    {
        return new DeliveryDto
        {
            Id = delivery.Id,
            OutboundId = delivery.OutboundId,
            OutboundNumber = delivery.Outbound?.OutboundNumber ?? "",
            DeliveryNumber = delivery.DeliveryNumber,
            Status = delivery.Status.ToString(),
            Carrier = delivery.Carrier,
            TrackingNumber = delivery.TrackingNumber,
            VehicleNumber = delivery.VehicleNumber,
            DriverName = delivery.DriverName,
            DriverPhone = delivery.DriverPhone,
            PickupDate = delivery.PickupDate,
            EstimatedDeliveryDate = delivery.EstimatedDeliveryDate,
            ActualDeliveryDate = delivery.ActualDeliveryDate,
            ShippingAddress = delivery.ShippingAddress,
            ShippingCity = delivery.ShippingCity,
            ShippingState = delivery.ShippingState,
            ShippingZipCode = delivery.ShippingZipCode,
            ShippingCountry = delivery.ShippingCountry,
            DeliveryNotes = delivery.DeliveryNotes,
            FailureReason = delivery.FailureReason,
            IsReturn = delivery.IsReturn,
            Events = delivery.DeliveryEvents?.Select(e => new DeliveryEventDto
            {
                Id = e.Id,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Location = e.Location,
                Notes = e.Notes
            }).ToList() ?? new List<DeliveryEventDto>(),
            CreatedAt = delivery.CreatedAt
        };
    }
}
