using MediatR;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;
using WMS.Domain.Enums;
using WMS.Payment.API.Application.Mappers;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Queries.GetAllPayments;

public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, Result<PagedResult<PaymentDto>>>
{
    private readonly WMSDbContext _context;

    public GetAllPaymentsQueryHandler(WMSDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<PaymentDto>>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .Include(p => p.Outbound)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PaymentStatus>(request.Status, out var paymentStatus))
        {
            query = query.Where(p => p.Status == paymentStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<PaymentDto>
        {
            Items = payments.Select(PaymentMapper.MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<PaymentDto>>.Success(result);
    }
}
