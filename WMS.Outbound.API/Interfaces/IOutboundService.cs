using WMS.Outbound.API.Common.Models;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Interfaces;

public interface IOutboundService
{
    Task<Result<OutboundDto>> GetByIdAsync(Guid id);
    Task<Result<PagedResult<OutboundDto>>> GetAllAsync(int pageNumber, int pageSize, string? status = null);
    Task<Result<OutboundDto>> CreateAsync(CreateOutboundDto dto, string currentUser);
    Task<Result<OutboundDto>> PickAsync(PickOutboundDto dto, string currentUser);
    Task<Result<OutboundDto>> ShipAsync(ShipOutboundDto dto, string currentUser);
    Task<Result> CancelAsync(Guid id, string currentUser);
}
