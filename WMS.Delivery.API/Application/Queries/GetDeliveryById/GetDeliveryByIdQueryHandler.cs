using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Delivery.API.Application.Mappers;
using WMS.Delivery.API.Common.Models;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Application.Queries.GetDeliveryById;

public class GetDeliveryByIdQueryHandler : IRequestHandler<GetDeliveryByIdQuery, Result<DeliveryDto>>
{
    private readonly WMSDbContext _context;

    public GetDeliveryByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeliveryDto>> Handle(GetDeliveryByIdQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries
            .Include(d => d.Outbound)
            .Include(d => d.DeliveryEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (delivery == null)
        {
            return Result<DeliveryDto>.Failure("Delivery not found");
        }

        return Result<DeliveryDto>.Success(DeliveryMapper.MapToDto(delivery));
    }
}
