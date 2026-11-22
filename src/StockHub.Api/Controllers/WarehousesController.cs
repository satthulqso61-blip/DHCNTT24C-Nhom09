using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockHub.Application.Common.Models;
using StockHub.Application.Features.Warehouses.DTOs;
using StockHub.Application.Features.Warehouses.Services;

namespace StockHub.Api.Controllers;

[Authorize]
public sealed class WarehousesController : ApiControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    /// <summary>
    /// Get paginated list of warehouses with optional search and active status filter.
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarehouses(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _warehouseService.GetWarehousesAsync(search, isActive, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all active warehouses (for dropdown selection).
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveWarehouses(CancellationToken cancellationToken)
    {
        var result = await _warehouseService.GetAllActiveWarehousesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get warehouse details by ID.
    /// Accessible by: Admin, WarehouseStaff, Accountant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWarehouseById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _warehouseService.GetWarehouseByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new warehouse.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _warehouseService.CreateWarehouseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWarehouseById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing warehouse.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _warehouseService.UpdateWarehouseAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivate/Delete a warehouse.
    /// Accessible by: Admin, WarehouseStaff.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,WarehouseStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWarehouse(Guid id, CancellationToken cancellationToken)
    {
        await _warehouseService.DeleteWarehouseAsync(id, cancellationToken);
        return NoContent();
    }
}
