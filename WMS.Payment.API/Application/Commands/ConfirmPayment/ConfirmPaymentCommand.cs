using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommand : IRequest<Result<PaymentDto>>
{
    public ConfirmPaymentDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
