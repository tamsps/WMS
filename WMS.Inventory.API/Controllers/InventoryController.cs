using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Inventory.API.Application.Queries.GetInventoryById;
using WMS.Inventory.API.Application.Queries.GetAllInventory;
using WMS.Inventory.API.Application.Queries.GetInventoryByProduct;
using WMS.Inventory.API.Application.Queries.GetInventoryByLocation;
using WMS.Inventory.API.Application.Queries.GetInventoryTransactions;
using WMS.Inventory.API.Application.Commands.CreateInventory;
using WMS.Inventory.API.Common.Models;
using WMS.Inventory.API.DTOs.Inventory;

namespace WMS.Inventory.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new inventory record
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "System";
        var command = new CreateInventoryCommand { Dto = dto, CurrentUser = currentUser };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction("GetById", new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Get inventory by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInventoryByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}

