using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Interfaces;

namespace WMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Get all inventory records with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _inventoryService.GetAllAsync(pageNumber, pageSize);
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
        var result = await _inventoryService.GetByIdAsync(id);
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
        var result = await _inventoryService.GetInventoryByProductAsync(productId);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get inventory levels across all locations with pagination and search
    /// </summary>
    [HttpGet("levels")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetLevels(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string? searchTerm = null)
    {
        var result = await _inventoryService.GetInventoryLevelsAsync(pageNumber, pageSize, searchTerm);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
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
        var result = await _inventoryService.GetTransactionsAsync(pageNumber, pageSize, productId, locationId);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get available quantity for a product at a specific location
    /// </summary>
    [HttpGet("availability")]
    [Authorize(Roles = "Admin,Manager,WarehouseStaff")]
    public async Task<IActionResult> GetAvailableQuantity(
        [FromQuery] Guid productId,
        [FromQuery] Guid locationId)
    {
        if (productId == Guid.Empty || locationId == Guid.Empty)
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "ProductId and LocationId are required" } });
        }

        var result = await _inventoryService.GetAvailableQuantityAsync(productId, locationId);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }
}
