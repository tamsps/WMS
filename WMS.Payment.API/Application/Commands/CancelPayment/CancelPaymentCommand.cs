using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.CancelPayment;

public class CancelPaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid PaymentId { get; set; }
    public string CurrentUser { get; set; } = null!;
}
