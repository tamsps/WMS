using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Entities;
using WMS.Domain.Enums;
using WMS.Domain.Interfaces;
using WMS.Inbound.API.Application.Mappers;
using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Application.Commands.CreateInbound;

public class CreateInboundCommandHandler : IRequestHandler<CreateInboundCommand, Result<InboundDto>>
{
    private readonly WMSDbContext _context;
    private readonly IRepository<WMS.Domain.Entities.Inbound> _inboundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInboundCommandHandler(
        WMSDbContext context,
        IRepository<WMS.Domain.Entities.Inbound> inboundRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _inboundRepository = inboundRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InboundDto>> Handle(CreateInboundCommand request, CancellationToken cancellationToken)
    {
        // Validate products and locations
        foreach (var item in request.Dto.Items)
        {
            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
            if (product == null || product.Status == ProductStatus.Inactive)
            {
                return Result<InboundDto>.Failure($"Product {item.ProductId} is invalid or inactive");
            }

            var location = await _context.Locations.FindAsync(new object[] { item.LocationId }, cancellationToken);
            if (location == null || !location.IsActive)
            {
                return Result<InboundDto>.Failure($"Location {item.LocationId} is invalid or inactive");
            }
        }

        var inbound = new WMS.Domain.Entities.Inbound
        {
            InboundNumber = await GenerateInboundNumberAsync(cancellationToken),
            ReferenceNumber = request.Dto.ReferenceNumber,
            Status = InboundStatus.Pending,
            ExpectedDate = request.Dto.ExpectedDate,
            SupplierName = request.Dto.SupplierName,
            SupplierCode = request.Dto.SupplierCode,
            Notes = request.Dto.Notes,
            CreatedBy = request.CurrentUser
        };

        foreach (var itemDto in request.Dto.Items)
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
                CreatedBy = request.CurrentUser
            });
        }

        await _inboundRepository.AddAsync(inbound, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch complete entity with includes
        var created = await _context.Inbounds
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.InboundItems)
                .ThenInclude(ii => ii.Location)
            .FirstOrDefaultAsync(i => i.Id == inbound.Id, cancellationToken);

        return Result<InboundDto>.Success(
            InboundMapper.MapToDto(created!), 
            "Inbound created successfully");
    }

    private async Task<string> GenerateInboundNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"IB-{today:yyyyMMdd}";
        
        var lastInbound = await _context.Inbounds
            .Where(i => i.InboundNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InboundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInbound == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumber = int.Parse(lastInbound.InboundNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D4}";
    }
}
