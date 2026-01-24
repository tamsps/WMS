using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Queries.GetPaymentById;

public class GetPaymentByIdQuery : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
