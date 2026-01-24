using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Queries.GetDeliveryByTrackingNumber;

public class GetDeliveryByTrackingNumberQueryHandler : IRequestHandler<GetDeliveryByTrackingNumberQuery, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;

    public GetDeliveryByTrackingNumberQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeliveryDto>> Handle(GetDeliveryByTrackingNumberQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.TrackingNumber == request.TrackingNumber, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure($"Delivery with tracking number '{request.TrackingNumber}' not found");
        }

        return Result<DeliveryDto>.Success(DeliveryMapper.MapToDto(delivery));
    }
}
