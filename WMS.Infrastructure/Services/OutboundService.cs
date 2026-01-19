using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Outbound;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class OutboundService : IOutboundService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Outbound> _outboundRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryService _inventoryService;

    public OutboundService(
        WMSDbContext context,
        IRepository<Outbound> outboundRepository,
        IUnitOfWork unitOfWork,
        IInventoryService inventoryService)
    {
        _context = context;
        _outboundRepository = outboundRepository;
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
    }

    public async Task<Result<OutboundDto>> GetByIdAsync(Guid id)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        return Result<OutboundDto>.Success(MapToDto(outbound));
    }

    public async Task<Result<PagedResult<OutboundDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null)
    {
        var query = _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .Include(o => o.Payment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OutboundStatus>(status, out var outboundStatus))
        {
            query = query.Where(o => o.Status == outboundStatus);
        }

        var totalCount = await query.CountAsync();
        var outbounds = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<OutboundDto>
        {
            Items = outbounds.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<OutboundDto>>.Success(result);
    }

    public async Task<Result<OutboundDto>> CreateAsync(CreateOutboundDto dto, string currentUser)
    {
        // Validate products, locations, and availability
        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Status == ProductStatus.Inactive)
            {
                return Result<OutboundDto>.Failure($"Product {item.ProductId} is invalid or inactive");
            }

            var location = await _context.Locations.FindAsync(item.LocationId);
            if (location == null || !location.IsActive)
            {
                return Result<OutboundDto>.Failure($"Location {item.LocationId} is invalid or inactive");
            }

            // Check inventory availability
            var availableResult = await _inventoryService.GetAvailableQuantityAsync(item.ProductId, item.LocationId);
            if (availableResult.Data < item.OrderedQuantity)
            {
                return Result<OutboundDto>.Failure(
                    $"Insufficient quantity for product {product.SKU} at location {location.Code}. " +
                    $"Available: {availableResult.Data}, Requested: {item.OrderedQuantity}");
            }
        }

        var outbound = new Outbound
        {
            OutboundNumber = await GenerateOutboundNumberAsync(),
            OrderNumber = dto.OrderNumber,
            Status = OutboundStatus.Pending,
            OrderDate = DateTime.UtcNow,
            CustomerName = dto.CustomerName,
            CustomerCode = dto.CustomerCode,
            ShippingAddress = dto.ShippingAddress,
            Notes = dto.Notes,
            CreatedBy = currentUser
        };

        foreach (var itemDto in dto.Items)
        {
            outbound.OutboundItems.Add(new OutboundItem
            {
                ProductId = itemDto.ProductId,
                LocationId = itemDto.LocationId,
                OrderedQuantity = itemDto.OrderedQuantity,
                PickedQuantity = 0,
                ShippedQuantity = 0,
                CreatedBy = currentUser
            });
        }

        await _outboundRepository.AddAsync(outbound);
        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(outbound.Id);
        return Result<OutboundDto>.Success(result.Data!, "Outbound created successfully");
    }

    public async Task<Result<OutboundDto>> PickAsync(PickOutboundDto dto, string currentUser)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .FirstOrDefaultAsync(o => o.Id == dto.OutboundId);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Pending)
        {
            return Result<OutboundDto>.Failure($"Cannot pick outbound in {outbound.Status} status");
        }

        foreach (var pickItem in dto.Items)
        {
            var outboundItem = outbound.OutboundItems.FirstOrDefault(oi => oi.Id == pickItem.OutboundItemId);
            if (outboundItem == null)
            {
                return Result<OutboundDto>.Failure($"Outbound item {pickItem.OutboundItemId} not found");
            }

            if (pickItem.PickedQuantity > outboundItem.OrderedQuantity)
            {
                return Result<OutboundDto>.Failure(
                    $"Picked quantity ({pickItem.PickedQuantity}) cannot exceed ordered quantity ({outboundItem.OrderedQuantity})");
            }

            // Check availability
            var availableResult = await _inventoryService.GetAvailableQuantityAsync(
                outboundItem.ProductId, outboundItem.LocationId);
            
            if (availableResult.Data < pickItem.PickedQuantity)
            {
                return Result<OutboundDto>.Failure(
                    $"Insufficient quantity for product {outboundItem.Product.SKU}. " +
                    $"Available: {availableResult.Data}, Requested: {pickItem.PickedQuantity}");
            }

            outboundItem.PickedQuantity = pickItem.PickedQuantity;
            outboundItem.LotNumber = pickItem.LotNumber;
            outboundItem.SerialNumber = pickItem.SerialNumber;
            outboundItem.UpdatedBy = currentUser;
            outboundItem.UpdatedAt = DateTime.UtcNow;
        }

        outbound.Status = OutboundStatus.Picked;
        outbound.UpdatedBy = currentUser;
        outbound.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(outbound.Id);
        return Result<OutboundDto>.Success(result.Data!, "Outbound picked successfully");
    }

    public async Task<Result<OutboundDto>> ShipAsync(ShipOutboundDto dto, string currentUser)
    {
        var outbound = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == dto.OutboundId);

        if (outbound == null)
        {
            return Result<OutboundDto>.Failure("Outbound not found");
        }

        if (outbound.Status != OutboundStatus.Picked && outbound.Status != OutboundStatus.Packed)
        {
            return Result<OutboundDto>.Failure($"Cannot ship outbound in {outbound.Status} status");
        }

        // Check payment status if payment is required
        if (outbound.PaymentId.HasValue)
        {
            if (outbound.Payment?.Status != PaymentStatus.Confirmed && 
                outbound.Payment?.PaymentType != PaymentType.COD &&
                outbound.Payment?.PaymentType != PaymentType.Postpaid)
            {
                return Result<OutboundDto>.Failure("Payment must be confirmed before shipping (except COD/Postpaid)");
            }
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Deduct inventory for all items
            foreach (var item in outbound.OutboundItems)
            {
                item.ShippedQuantity = item.PickedQuantity;
                item.UpdatedBy = currentUser;
                item.UpdatedAt = DateTime.UtcNow;

                var inventoryResult = await _inventoryService.UpdateInventoryAsync(
                    item.ProductId,
                    item.LocationId,
                    item.ShippedQuantity,
                    TransactionType.Outbound,
                    outbound.OutboundNumber,
                    outbound.Id,
                    currentUser
                );

                if (!inventoryResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<OutboundDto>.Failure(inventoryResult.Errors.FirstOrDefault() ?? "Failed to update inventory");
                }
            }

            outbound.Status = OutboundStatus.Shipped;
            outbound.ShipDate = DateTime.UtcNow;
            outbound.UpdatedBy = currentUser;
            outbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitTransactionAsync();

            var result = await GetByIdAsync(outbound.Id);
            return Result<OutboundDto>.Success(result.Data!, "Outbound shipped successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<OutboundDto>.Failure($"Failed to ship outbound: {ex.Message}");
        }
    }

    public async Task<Result> CancelAsync(Guid id, string currentUser)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
        {
            return Result.Failure("Outbound not found");
        }

        if (outbound.Status == OutboundStatus.Shipped)
        {
            return Result.Failure("Cannot cancel shipped outbound");
        }

        outbound.Status = OutboundStatus.Cancelled;
        outbound.UpdatedBy = currentUser;
        outbound.UpdatedAt = DateTime.UtcNow;

        await _outboundRepository.UpdateAsync(outbound);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Outbound cancelled successfully");
    }

    private async Task<string> GenerateOutboundNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"OB-{today:yyyyMMdd}";
        
        var lastOutbound = await _context.Outbounds
            .Where(o => o.OutboundNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OutboundNumber)
            .FirstOrDefaultAsync();

        if (lastOutbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastOutbound.OutboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    private static OutboundDto MapToDto(Outbound outbound)
    {
        return new OutboundDto
        {
            Id = outbound.Id,
            OutboundNumber = outbound.OutboundNumber,
            OrderNumber = outbound.OrderNumber,
            Status = outbound.Status.ToString(),
            OrderDate = outbound.OrderDate,
            ShipDate = outbound.ShipDate,
            CustomerName = outbound.CustomerName,
            CustomerCode = outbound.CustomerCode,
            ShippingAddress = outbound.ShippingAddress,
            Notes = outbound.Notes,
            PaymentId = outbound.PaymentId,
            PaymentStatus = outbound.Payment?.Status.ToString(),
            Items = outbound.OutboundItems.Select(oi => new OutboundItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductSKU = oi.Product.SKU,
                ProductName = oi.Product.Name,
                LocationId = oi.LocationId,
                LocationCode = oi.Location.Code,
                OrderedQuantity = oi.OrderedQuantity,
                PickedQuantity = oi.PickedQuantity,
                ShippedQuantity = oi.ShippedQuantity,
                LotNumber = oi.LotNumber,
                SerialNumber = oi.SerialNumber,
                Notes = oi.Notes
            }).ToList(),
            CreatedAt = outbound.CreatedAt
        };
    }
}
