using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.DTOs.Delivery;
using WMS.Application.Interfaces;
using System.Security.Claims;

namespace WMS.Delivery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Get all deliveries with pagination and optional status filter
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _deliveryService.GetAllAsync(pageNumber, pageSize, status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get delivery by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _deliveryService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get delivery by tracking number (public endpoint for customer tracking)
    /// </summary>
    [HttpGet("track/{trackingNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackByNumber(string trackingNumber)
    {
        var result = await _deliveryService.GetByTrackingNumberAsync(trackingNumber);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new delivery
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _deliveryService.CreateAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update delivery status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDeliveryStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.DeliveryId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _deliveryService.UpdateStatusAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Complete delivery (mark as delivered)
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteDeliveryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.DeliveryId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _deliveryService.CompleteAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Mark delivery as failed
    /// </summary>
    [HttpPost("{id}/fail")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailDeliveryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.DeliveryId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _deliveryService.FailAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Add delivery event (tracking update)
    /// </summary>
    [HttpPost("{id}/events")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> AddEvent(Guid id, [FromBody] AddDeliveryEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.DeliveryId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _deliveryService.AddEventAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get delivery statistics (summary)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? status = null)
    {
        // Get all deliveries with the status filter
        var result = await _deliveryService.GetAllAsync(1, int.MaxValue, status);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var stats = new
        {
            TotalCount = result.Data!.TotalCount,
            PendingCount = result.Data.Items.Count(d => d.Status == "Pending"),
            InTransitCount = result.Data.Items.Count(d => d.Status == "InTransit"),
            DeliveredCount = result.Data.Items.Count(d => d.Status == "Delivered"),
            FailedCount = result.Data.Items.Count(d => d.Status == "Failed"),
            ReturnedCount = result.Data.Items.Count(d => d.Status == "Returned"),
            CancelledCount = result.Data.Items.Count(d => d.Status == "Cancelled"),
            OnTimeDeliveryRate = CalculateOnTimeRate(result.Data.Items)
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }

    private static double CalculateOnTimeRate(List<DeliveryDto> deliveries)
    {
        var completedDeliveries = deliveries.Where(d => d.Status == "Delivered" && d.ActualDeliveryDate.HasValue).ToList();
        if (!completedDeliveries.Any()) return 0;

        var onTimeCount = completedDeliveries.Count(d => 
            !d.EstimatedDeliveryDate.HasValue || 
            d.ActualDeliveryDate!.Value <= d.EstimatedDeliveryDate.Value);

        return Math.Round((double)onTimeCount / completedDeliveries.Count * 100, 2);
    }
}

