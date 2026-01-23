using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.DTOs.Outbound;
using WMS.Application.Interfaces;
using System.Security.Claims;

namespace WMS.Outbound.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OutboundController : ControllerBase
{
    private readonly IOutboundService _outboundService;

    public OutboundController(IOutboundService outboundService)
    {
        _outboundService = outboundService;
    }

    /// <summary>
    /// Get all outbound orders with pagination and optional status filter
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _outboundService.GetAllAsync(pageNumber, pageSize, status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get outbound order by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _outboundService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new outbound order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Create([FromBody] CreateOutboundDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _outboundService.CreateAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Pick items for outbound order
    /// </summary>
    [HttpPost("{id}/pick")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Pick(Guid id, [FromBody] PickOutboundDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.OutboundId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _outboundService.PickAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Ship outbound order (deducts inventory)
    /// </summary>
    [HttpPost("{id}/ship")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Ship(Guid id, [FromBody] ShipOutboundDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.OutboundId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _outboundService.ShipAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Cancel an outbound order
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _outboundService.CancelAsync(id, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get outbound statistics (summary)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? status = null)
    {
        // Get all outbounds with the status filter
        var result = await _outboundService.GetAllAsync(1, int.MaxValue, status);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var stats = new
        {
            TotalCount = result.Data!.TotalCount,
            PendingCount = result.Data.Items.Count(i => i.Status == "Pending"),
            PickedCount = result.Data.Items.Count(i => i.Status == "Picked"),
            PackedCount = result.Data.Items.Count(i => i.Status == "Packed"),
            ShippedCount = result.Data.Items.Count(i => i.Status == "Shipped"),
            CancelledCount = result.Data.Items.Count(i => i.Status == "Cancelled"),
            TotalItems = result.Data.Items.Sum(i => i.Items.Sum(item => item.OrderedQuantity)),
            TotalShippedItems = result.Data.Items.Sum(i => i.Items.Sum(item => item.ShippedQuantity))
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }
}

