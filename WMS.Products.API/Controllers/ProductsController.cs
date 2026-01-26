using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using WMS.Products.API.Application.Commands.CreateProduct;
using WMS.Products.API.Application.Commands.UpdateProduct;
using WMS.Products.API.Application.Commands.ActivateProduct;
using WMS.Products.API.Application.Commands.DeactivateProduct;
using WMS.Products.API.Application.Queries.GetProductById;
using WMS.Products.API.Application.Queries.GetAllProducts;
using WMS.Products.API.Application.Queries.GetActiveProducts;
using WMS.Products.API.Application.Queries.GetProductBySku;
using WMS.Products.API.DTOs.Product;
using WMS.Domain.Enums;

namespace WMS.Products.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null)
    {
        ProductStatus? productStatus = null;
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<ProductStatus>(status, true, out var parsedStatus))
            {
                productStatus = parsedStatus;
            }
        }

        var query = new GetAllProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Status = productStatus
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get all active products (convenience endpoint for transactions)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetActiveProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySKU(string sku)
    {
        var query = new GetProductBySkuQuery { SKU = sku };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    //[Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new CreateProductCommand
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
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }

        var currentUser = User.Identity?.Name ?? "System";

        var command = new UpdateProductCommand
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
    /// Activate a product
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new ActivateProductCommand
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
    /// Deactivate a product
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var command = new DeactivateProductCommand
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

