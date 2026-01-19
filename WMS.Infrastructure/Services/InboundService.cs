using Microsoft.EntityFrameworkCore;
using WMS.Application.Common.Models;
using WMS.Application.DTOs.Inbound;
using WMS.Application.Interfaces;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Infrastructure.Data;

namespace WMS.Infrastructure.Services;

public class InboundService : IInboundService
{
    private readonly WMSDbContext _context;
    private readonly IRepository<Inbound> _inboundRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryService _inventoryService;

    public InboundService(
        WMSDbContext context,
        IRepository<Inbound> inboundRepository,
        IUnitOfWork unitOfWork,
        IInventoryService inventoryService)
    {
        _context = context;
        _inboundRepository = inboundRepository;
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
    }

    public async Task<Result<InboundDto>> GetByIdAsync(Guid id)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        return Result<InboundDto>.Success(MapToDto(inbound));
    }

    public async Task<Result<PagedResult<InboundDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null)
    {
        var query = _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InboundStatus>(status, out var inboundStatus))
        {
            query = query.Where(i => i.Status == inboundStatus);
        }

        var totalCount = await query.CountAsync();
        var inbounds = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<InboundDto>
        {
            Items = inbounds.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<PagedResult<InboundDto>>.Success(result);
    }

    public async Task<Result<InboundDto>> CreateAsync(CreateInboundDto dto, string currentUser)
    {
        // Validate products and locations
        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.Status == ProductStatus.Inactive)
            {
                return Result<InboundDto>.Failure($"Product {item.ProductId} is invalid or inactive");
            }

            var location = await _context.Locations.FindAsync(item.LocationId);
            if (location == null || !location.IsActive)
            {
                return Result<InboundDto>.Failure($"Location {item.LocationId} is invalid or inactive");
            }
        }

        var inbound = new Inbound
        {
            InboundNumber = await GenerateInboundNumberAsync(),
            ReferenceNumber = dto.ReferenceNumber,
            Status = InboundStatus.Pending,
            ExpectedDate = dto.ExpectedDate,
            SupplierName = dto.SupplierName,
            SupplierCode = dto.SupplierCode,
            Notes = dto.Notes,
            CreatedBy = currentUser
        };

        foreach (var itemDto in dto.Items)
        {
            inbound.InboundItems.Add(new InboundItem
            {
                ProductId = itemDto.ProductId,
                LocationId = itemDto.LocationId,
                ExpectedQuantity = itemDto.ExpectedQuantity,
                ReceivedQuantity = 0,
                LotNumber = itemDto.LotNumber,
                ExpiryDate = itemDto.ExpiryDate,
                Notes = itemDto.Notes,
                CreatedBy = currentUser
            });
        }

        await _inboundRepository.AddAsync(inbound);
        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(inbound.Id);
        return Result<InboundDto>.Success(result.Data!, "Inbound created successfully");
    }

    public async Task<Result<InboundDto>> ReceiveAsync(ReceiveInboundDto dto, string currentUser)
    {
        var inbound = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == dto.InboundId);

        if (inbound == null)
        {
            return Result<InboundDto>.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Pending)
        {
            return Result<InboundDto>.Failure($"Cannot receive inbound in {inbound.Status} status");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var receiveItem in dto.Items)
            {
                var inboundItem = inbound.InboundItems.FirstOrDefault(ii => ii.Id == receiveItem.InboundItemId);
                if (inboundItem == null)
                {
                    return Result<InboundDto>.Failure($"Inbound item {receiveItem.InboundItemId} not found");
                }

                inboundItem.ReceivedQuantity = receiveItem.ReceivedQuantity;
                inboundItem.DamagedQuantity = receiveItem.DamagedQuantity;
                if (!string.IsNullOrWhiteSpace(receiveItem.Notes))
                {
                    inboundItem.Notes = receiveItem.Notes;
                }
                inboundItem.UpdatedBy = currentUser;
                inboundItem.UpdatedAt = DateTime.UtcNow;

                // Update inventory for received quantity (excluding damaged)
                var goodQuantity = receiveItem.ReceivedQuantity - (receiveItem.DamagedQuantity ?? 0);
                if (goodQuantity > 0)
                {
                    var inventoryResult = await _inventoryService.UpdateInventoryAsync(
                        inboundItem.ProductId,
                        inboundItem.LocationId,
                        goodQuantity,
                        TransactionType.Inbound,
                        inbound.InboundNumber,
                        inbound.Id,
                        currentUser
                    );

                    if (!inventoryResult.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<InboundDto>.Failure(inventoryResult.Errors.FirstOrDefault() ?? "Failed to update inventory");
                    }
                }
            }

            inbound.Status = InboundStatus.Received;
            inbound.ReceivedDate = DateTime.UtcNow;
            inbound.UpdatedBy = currentUser;
            inbound.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitTransactionAsync();

            var result = await GetByIdAsync(inbound.Id);
            return Result<InboundDto>.Success(result.Data!, "Inbound received successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<InboundDto>.Failure($"Failed to receive inbound: {ex.Message}");
        }
    }

    public async Task<Result> CancelAsync(Guid id, string currentUser)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
        {
            return Result.Failure("Inbound not found");
        }

        if (inbound.Status != InboundStatus.Pending)
        {
            return Result.Failure($"Cannot cancel inbound in {inbound.Status} status");
        }

        inbound.Status = InboundStatus.Cancelled;
        inbound.UpdatedBy = currentUser;
        inbound.UpdatedAt = DateTime.UtcNow;

        await _inboundRepository.UpdateAsync(inbound);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success("Inbound cancelled successfully");
    }

    private async Task<string> GenerateInboundNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"IB-{today:yyyyMMdd}";
        
        var lastInbound = await _context.Inbounds
            .Where(i => i.InboundNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InboundNumber)
            .FirstOrDefaultAsync();

        if (lastInbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastInbound.InboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }

    private static InboundDto MapToDto(Inbound inbound)
    {
        return new InboundDto
        {
            Id = inbound.Id,
            InboundNumber = inbound.InboundNumber,
            ReferenceNumber = inbound.ReferenceNumber,
            Status = inbound.Status.ToString(),
            ExpectedDate = inbound.ExpectedDate,
            ReceivedDate = inbound.ReceivedDate,
            SupplierName = inbound.SupplierName,
            SupplierCode = inbound.SupplierCode,
            Notes = inbound.Notes,
            Items = inbound.InboundItems.Select(ii => new InboundItemDto
            {
                Id = ii.Id,
                ProductId = ii.ProductId,
                ProductSKU = ii.Product.SKU,
                ProductName = ii.Product.Name,
                LocationId = ii.LocationId,
                LocationCode = ii.Location.Code,
                ExpectedQuantity = ii.ExpectedQuantity,
                ReceivedQuantity = ii.ReceivedQuantity,
                DamagedQuantity = ii.DamagedQuantity,
                LotNumber = ii.LotNumber,
                ExpiryDate = ii.ExpiryDate,
                Notes = ii.Notes
            }).ToList(),
            CreatedAt = inbound.CreatedAt
        };
    }
}
