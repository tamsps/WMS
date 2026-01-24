using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Payment.API.Application.Commands.CreatePayment;
using WMS.Payment.API.Application.Commands.ConfirmPayment;
using WMS.Payment.API.Application.Commands.CancelPayment;
using WMS.Payment.API.Application.Queries.GetPaymentById;
using WMS.Payment.API.Application.Queries.GetAllPayments;
using WMS.Payment.API.DTOs.Payment;

namespace WMS.Payment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all payments with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = new GetAllPaymentsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status
        };

        var result = await _mediator.Send(query);

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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetPaymentByIdQuery { Id = id };
        var result = await _mediator.Send(query);

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
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CreatePaymentCommand
        {
            Dto = dto,
            CurrentUser = currentUser
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Confirm payment
    /// </summary>
    [HttpPost("confirm")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new ConfirmPaymentCommand
        {
            Dto = dto,
            CurrentUser = currentUser
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Cancel payment
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CancelPaymentCommand
        {
            PaymentId = id,
            CurrentUser = currentUser
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

