using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Payment.API.Application.Mappers;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly WMSDbContext _context;

    public GetPaymentByIdQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Outbound)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (payment == null)
        {
            return Result<PaymentDto>.Failure("Payment not found");
        }

        return Result<PaymentDto>.Success(PaymentMapper.MapToDto(payment));
    }
}
