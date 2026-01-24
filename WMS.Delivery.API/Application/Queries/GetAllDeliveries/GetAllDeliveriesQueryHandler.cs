using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Queries.GetAllDeliveries;

public class GetAllDeliveriesQueryHandler : IRequestHandler<GetAllDeliveriesQuery, Result<PagedResult<DeliveryDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllDeliveriesQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<DeliveryDto>>> Handle(GetAllDeliveriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DeliveryStatus>(request.Status, out var deliveryStatus))
        {
            query = query.Where(d => d.Status == deliveryStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var deliveries = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DeliveryDto>
        {
            Items = deliveries.Select(DeliveryMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<DeliveryDto>>.Success(result);
    }
}
