using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Outbound.API.Application.Commands.CreateOutbound;
using WMS.Outbound.API.Application.Commands.PickOutbound;
using WMS.Outbound.API.Application.Commands.ShipOutbound;
using WMS.Outbound.API.Application.Commands.CancelOutbound;
using WMS.Outbound.API.Application.Queries.GetOutboundById;
using WMS.Outbound.API.Application.Queries.GetAllOutbounds;
using WMS.Outbound.API.DTOs.Outbound;

namespace WMS.Outbound.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OutboundController : ControllerBase
{
    private readonly IMediator _mediator;

    public OutboundController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all outbound shipments with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = new GetAllOutboundsQuery
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
    /// Get outbound shipment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOutboundByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new outbound shipment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Create([FromBody] CreateOutboundDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CreateOutboundCommand
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
    /// Pick outbound items (reserves inventory)
    /// </summary>
    [HttpPost("pick")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Pick([FromBody] PickOutboundDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new PickOutboundCommand
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
    /// Ship outbound (deducts inventory)
    /// </summary>
    [HttpPost("ship")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Ship([FromBody] ShipOutboundDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new ShipOutboundCommand
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
    /// Cancel outbound shipment
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CancelOutboundCommand
        {
            OutboundId = id,
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

