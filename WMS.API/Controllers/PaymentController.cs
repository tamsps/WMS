using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.DTOs.Payment;
using WMS.Application.Interfaces;
using System.Security.Claims;

namespace WMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Get all payments with pagination and optional status filter
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _paymentService.GetAllAsync(pageNumber, pageSize, status);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _paymentService.CreateAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Initiate payment with gateway
    /// </summary>
    [HttpPost("{id}/initiate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Initiate(Guid id, [FromBody] InitiatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.PaymentId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _paymentService.InitiateAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Confirm payment
    /// </summary>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmPaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != dto.PaymentId)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ID mismatch" } });
        }

        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        var result = await _paymentService.ConfirmAsync(dto, currentUser);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Process payment webhook from external gateway
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous] // Webhooks come from external systems
    public async Task<IActionResult> ProcessWebhook([FromBody] PaymentWebhookDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // In production, you should validate webhook signature/token here
        var result = await _paymentService.ProcessWebhookAsync(dto);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Check if an outbound can be shipped based on payment status
    /// </summary>
    [HttpGet("can-ship/{outboundId}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> CanShip(Guid outboundId)
    {
        var result = await _paymentService.CanShipAsync(outboundId);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get payment statistics (summary)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? status = null)
    {
        // Get all payments with the status filter
        var result = await _paymentService.GetAllAsync(1, int.MaxValue, status);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var stats = new
        {
            TotalCount = result.Data!.TotalCount,
            PendingCount = result.Data.Items.Count(p => p.Status == "Pending"),
            ConfirmedCount = result.Data.Items.Count(p => p.Status == "Confirmed"),
            FailedCount = result.Data.Items.Count(p => p.Status == "Failed"),
            CancelledCount = result.Data.Items.Count(p => p.Status == "Cancelled"),
            TotalAmount = result.Data.Items.Sum(p => p.Amount),
            ConfirmedAmount = result.Data.Items.Where(p => p.Status == "Confirmed").Sum(p => p.Amount),
            PendingAmount = result.Data.Items.Where(p => p.Status == "Pending").Sum(p => p.Amount)
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }
}
