using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Inventory.API.Application.Queries.GetInventoryById;
using WMS.Inventory.API.Application.Queries.GetAllInventory;
using WMS.Inventory.API.Application.Queries.GetInventoryByProduct;
using WMS.Inventory.API.Application.Queries.GetInventoryByLocation;
using WMS.Inventory.API.Application.Queries.GetInventoryTransactions;

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
    /// Get all inventory records with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetAllInventoryQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
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

    /// <summary>
    /// Get inventory by product ID
    /// </summary>
    [HttpGet("product/{productId}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var query = new GetInventoryByProductQuery { ProductId = productId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get inventory by location ID
    /// </summary>
    [HttpGet("location/{locationId}")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetByLocation(Guid locationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetInventoryByLocationQuery
        {
            LocationId = locationId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get inventory transactions with optional filters
    /// </summary>
    [HttpGet("transactions")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? locationId = null)
    {
        var query = new GetInventoryTransactionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            ProductId = productId,
            LocationId = locationId
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

