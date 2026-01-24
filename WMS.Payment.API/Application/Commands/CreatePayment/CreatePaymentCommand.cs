using MediatR;
using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Application.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<Result<PaymentDto>>
{
    public CreatePaymentDto Dto { get; set; } = null!;
    public string CurrentUser { get; set; } = null!;
}
