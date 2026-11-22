using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockHub.Application.Common.Models;
using StockHub.Application.Features.Products.DTOs;
using StockHub.Application.Features.Products.Services;

namespace StockHub.Api.Controllers;

[Authorize]
public sealed class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get paginated list of products with optional search query.
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductsAsync(search, page, pageSize, includeDeleted, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all active products without pagination (for select dropdowns).
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken)
    {
        var result = await _productService.GetAllProductsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get product details by ID.
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing product.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateProductAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete a product (sets IsDeleted = true, preserves historical records).
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Restore a soft-deleted product.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RestoreProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.RestoreProductAsync(id, cancellationToken);
        return Ok(result);
    }
}
