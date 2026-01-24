using WMS.Payment.API.Common.Models;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<PaymentDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null);
    Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto dto, string currentUser);
    Task<Result<PaymentDto>> InitiateAsync(InitiatePaymentDto dto, string currentUser);
    Task<Result<PaymentDto>> ConfirmAsync(ConfirmPaymentDto dto, string currentUser);
    Task<Result> ProcessWebhookAsync(PaymentWebhookDto dto);
    Task<Result<bool>> CanShipAsync(Guid outboundId);
}
