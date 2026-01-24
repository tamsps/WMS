using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using WMS.Inbound.API.Application.Commands.CreateInbound;
using WMS.Inbound.API.Application.Commands.ReceiveInbound;
using WMS.Inbound.API.Application.Commands.CancelInbound;
using WMS.Inbound.API.Application.Queries.GetInboundById;
using WMS.Inbound.API.Application.Queries.GetAllInbounds;
using WMS.Inbound.API.DTOs.Inbound;

namespace WMS.Inbound.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InboundController : ControllerBase
{
    private readonly IMediator _mediator;

    public InboundController(IMediator mediator)
    {
        _mediator = mediator;
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
        var query = new GetAllInboundsQuery
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
    /// Get inbound shipment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInboundByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
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
        
        var command = new CreateInboundCommand 
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
        
        var command = new ReceiveInboundCommand 
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
    /// Cancel an inbound shipment
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        
        var command = new CancelInboundCommand 
        { 
            Id = id, 
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
    /// Get inbound statistics (summary)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? status = null)
    {
        // Get all inbounds using CQRS query
        var query = new GetAllInboundsQuery
        {
            PageNumber = 1,
            PageSize = int.MaxValue,
            Status = status
        };
        
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        var stats = new
        {
            TotalCount = result.Data!.TotalCount,
            PendingCount = result.Data.Items.Count(i => i.Status == "Pending"),
            ReceivedCount = result.Data.Items.Count(i => i.Status == "Received"),
            CancelledCount = result.Data.Items.Count(i => i.Status == "Cancelled")
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }
}

