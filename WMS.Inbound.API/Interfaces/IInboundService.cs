using WMS.Inbound.API.Common.Models;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Interfaces;

public interface IInboundService
{
    Task<Result<InboundDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<InboundDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null);
    Task<Result<InboundDto>> CreateAsync(CreateInboundDto dto, string currentUser);
    Task<Result<InboundDto>> ReceiveAsync(ReceiveInboundDto dto, string currentUser);
    Task<Result> CancelAsync(Guid id, string currentUser);
}
