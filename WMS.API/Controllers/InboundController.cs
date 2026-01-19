using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.DTOs.Inbound;
using WMS.Application.Interfaces;
using System.Security.Claims;

namespace WMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InboundController : ControllerBase
{
    private readonly IInboundService _inboundService;

    public InboundController(IInboundService inboundService)
    {
        _inboundService = inboundService;
    }

    /// <summary>
    /// Get all inbound shipments with pagination and optional status filter
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _inboundService.GetAllAsync(pageNumber, pageSize, status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get inbound shipment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _inboundService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new inbound shipment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Create([FromBody] CreateInboundDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _inboundService.CreateAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Receive inbound shipment items (updates inventory)
    /// </summary>
    [HttpPost("{id}/receive")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveInboundDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.InboundId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _inboundService.ReceiveAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Cancel an inbound shipment
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _inboundService.CancelAsync(id, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get inbound statistics (summary)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? status = null)
    {
        // Get all inbounds with the status filter
        var result = await _inboundService.GetAllAsync(1, int.MaxValue, status);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var stats = new
        {
            TotalCount = result.Data!.TotalCount,
            PendingCount = result.Data.Items.Count(i => i.Status == "Pending"),
            InProgressCount = result.Data.Items.Count(i => i.Status == "InProgress"),
            CompletedCount = result.Data.Items.Count(i => i.Status == "Completed"),
            CancelledCount = result.Data.Items.Count(i => i.Status == "Cancelled")
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }
}
