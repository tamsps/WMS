using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Delivery.API.Application.Commands.CreateDelivery;
using WMS.Delivery.API.Application.Commands.UpdateDeliveryStatus;
using WMS.Delivery.API.Application.Commands.CompleteDelivery;
using WMS.Delivery.API.Application.Commands.FailDelivery;
using WMS.Delivery.API.Application.Commands.AddDeliveryEvent;
using WMS.Delivery.API.Application.Queries.GetDeliveryById;
using WMS.Delivery.API.Application.Queries.GetAllDeliveries;
using WMS.Delivery.API.Application.Queries.GetDeliveryByTrackingNumber;
using WMS.Delivery.API.DTOs.Delivery;

namespace WMS.Delivery.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all deliveries with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = new GetAllDeliveriesQuery
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
    /// Get delivery by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetDeliveryByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get delivery by tracking number
    /// </summary>
    [HttpGet("tracking/{trackingNumber}")]
    [AllowAnonymous] // Allow public tracking
    public async Task<IActionResult> GetByTrackingNumber(string trackingNumber)
    {
        var query = new GetDeliveryByTrackingNumberQuery { TrackingNumber = trackingNumber };
        var result = await _mediator.Send(query);

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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CreateDeliveryCommand
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
    /// Update delivery status
    /// </summary>
    [HttpPut("status")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateDeliveryStatusDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new UpdateDeliveryStatusCommand
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
    /// Complete delivery
    /// </summary>
    [HttpPost("complete")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Complete([FromBody] CompleteDeliveryDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CompleteDeliveryCommand
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
    /// Mark delivery as failed
    /// </summary>
    [HttpPost("fail")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Fail([FromBody] FailDeliveryDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new FailDeliveryCommand
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
    /// Add delivery event
    /// </summary>
    [HttpPost("event")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> AddEvent([FromBody] AddDeliveryEventDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new AddDeliveryEventCommand
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
}

