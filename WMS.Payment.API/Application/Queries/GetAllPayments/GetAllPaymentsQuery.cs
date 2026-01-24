using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Queries.GetAllPayments;

public class GetAllPaymentsQuery : IRequest<Result<PagedResult<PaymentDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
}
