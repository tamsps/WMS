using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Locations.API.Application.Commands.CreateLocation;
using WMS.Locations.API.Application.Commands.UpdateLocation;
using WMS.Locations.API.Application.Commands.ActivateLocation;
using WMS.Locations.API.Application.Commands.DeactivateLocation;
using WMS.Locations.API.Application.Queries.GetLocationById;
using WMS.Locations.API.Application.Queries.GetAllLocations;
using WMS.Locations.API.Application.Queries.GetLocationByCode;
using WMS.Locations.API.DTOs.Location;

namespace WMS.Locations.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all locations with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetAllLocationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get location by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetLocationByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get location by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var query = new GetLocationByCodeQuery { Code = code };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CreateLocationCommand
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
    /// Update an existing location
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }

        var currentUser = User.Identity?.Name ?? "System";

        var command = new UpdateLocationCommand
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
    /// Activate a location
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new ActivateLocationCommand
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
    /// Deactivate a location
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new DeactivateLocationCommand
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
}

