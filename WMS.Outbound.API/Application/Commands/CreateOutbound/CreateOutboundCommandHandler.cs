using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Outbound.API.Application.Mappers;
using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Application.Commands.CreateOutbound;

public class CreateOutboundCommandHandler : IRequestHandler<CreateOutboundCommand, Result<OutboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<WMS.Domain.Entities.Outbound> _outboundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOutboundCommandHandler(
        WMSDbContext context,
        IRepository<WMS.Domain.Entities.Outbound> outboundRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _outboundRepository = outboundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OutboundDto>> Handle(CreateOutboundCommand request, CancellationToken cancellationToken)
    {
        // Validate items
        if (request.Dto.Items == null || !request.Dto.Items.Any())
        {
            return Result<OutboundDto>.Failure("At least one item is required");
        }

        // Validate all products and locations exist and are active
        foreach (var itemDto in request.Dto.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, cancellationToken);
            if (product == null)
            {
                return Result<OutboundDto>.Failure($"Product with ID {itemDto.ProductId} not found");
            }

            // Validate product is active
            if (product.Status == ProductStatus.Inactive)
            {
                return Result<OutboundDto>.Failure($"Product {product.SKU} is inactive and cannot be used in transactions");
            }

            var location = await _context.Locations.FindAsync(new object[] { itemDto.LocationId }, cancellationToken);
            if (location == null)
            {
                return Result<OutboundDto>.Failure($"Location with ID {itemDto.LocationId} not found");
            }

            // Validate location is active
            if (!location.IsActive)
            {
                return Result<OutboundDto>.Failure($"Location {location.Code} is inactive and cannot be used in transactions");
            }

            // Check inventory availability
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == itemDto.ProductId && i.LocationId == itemDto.LocationId, cancellationToken);

            if (inventory == null || inventory.QuantityAvailable < itemDto.OrderedQuantity)
            {
                return Result<OutboundDto>.Failure($"Insufficient inventory for product {product.SKU} at location {location.Code}");
            }
        }

        var outbound = new WMS.Domain.Entities.Outbound
        {
            OutboundNumber = await GenerateOutboundNumberAsync(cancellationToken),
            OrderNumber = request.Dto.OrderNumber,
            Status = OutboundStatus.Pending,
            OrderDate = DateTime.UtcNow,
            CustomerName = request.Dto.CustomerName,
            CustomerCode = request.Dto.CustomerCode,
            ShippingAddress = request.Dto.ShippingAddress,
            Notes = request.Dto.Notes,
            CreatedBy = request.CurrentUser
        };

        // Add items
        foreach (var itemDto in request.Dto.Items)
        {
            outbound.OutboundItems.Add(new WMS.Domain.Entities.OutboundItem
            {
                ProductId = itemDto.ProductId,
                LocationId = itemDto.LocationId,
                OrderedQuantity = itemDto.OrderedQuantity,
                PickedQuantity = 0,
                ShippedQuantity = 0,
                CreatedBy = request.CurrentUser
            });
        }

        await _outboundRepository.AddAsync(outbound, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with includes
        var created = await _context.Outbounds
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OutboundItems)
                .ThenInclude(oi => oi.Location)
            .FirstOrDefaultAsync(o => o.Id == outbound.Id, cancellationToken);

        return Result<OutboundDto>.Success(
            OutboundMapper.MapToDto(created!),
            "Outbound order created successfully");
    }

    private async Task<string> GenerateOutboundNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"OUT-{today:yyyyMMdd}";

        var lastOutbound = await _context.Outbounds
            .Where(o => o.OutboundNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OutboundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOutbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastOutbound.OutboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
