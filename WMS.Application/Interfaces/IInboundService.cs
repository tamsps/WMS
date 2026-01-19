using WMS.Application.Common.Models;
using WMS.Application.DTOs.Inbound;

namespace WMS.Application.Interfaces;

public interface IInboundService
{
    Task<Result<InboundDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<InboundDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null);
    Task<Result<InboundDto>> CreateAsync(CreateInboundDto dto, string currentUser);
    Task<Result<InboundDto>> ReceiveAsync(ReceiveInboundDto dto, string currentUser);
    Task<Result> CancelAsync(Guid id, string currentUser);
}
