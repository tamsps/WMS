using WMS.Application.Common.Models;
using WMS.Application.DTOs.Delivery;

namespace WMS.Application.Interfaces;

public interface IDeliveryService
{
    Task<Result<DeliveryDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<DeliveryDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null);
    Task<Result<DeliveryDto>> CreateAsync(CreateDeliveryDto dto, string currentUser);
    Task<Result<DeliveryDto>> UpdateStatusAsync(UpdateDeliveryStatusDto dto, string currentUser);
    Task<Result<DeliveryDto>> CompleteAsync(CompleteDeliveryDto dto, string currentUser);
    Task<Result<DeliveryDto>> FailAsync(FailDeliveryDto dto, string currentUser);
    Task<Result<DeliveryDto>> GetByTrackingNumberAsync(string trackingNumber);
    Task<Result<DeliveryDto>> AddEventAsync(AddDeliveryEventDto dto, string currentUser);
}
