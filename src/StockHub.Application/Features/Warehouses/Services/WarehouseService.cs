using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Common.Interfaces;
using StockHub.Application.Common.Models;
using StockHub.Application.Features.Warehouses.DTOs;
using StockHub.Domain.Entities;

namespace StockHub.Application.Features.Warehouses.Services;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IApplicationDbContext _context;

    public WarehouseService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WarehouseDto>> GetWarehousesAsync(
        string? search,
        bool? isActive,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Warehouses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(w => w.Code.ToLower().Contains(searchLower) || w.Name.ToLower().Contains(searchLower));
        }

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => MapToDto(w))
            .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Code)
            .Select(w => MapToDto(w))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<WarehouseDto> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse == null)
        {
            throw new AppException($"Warehouse with ID '{id}' was not found.", "WAREHOUSE_NOT_FOUND", 404);
        }

        return MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var codeExists = await _context.Warehouses
            .AnyAsync(w => w.Code == normalizedCode, cancellationToken);

        if (codeExists)
        {
            throw new AppException($"Warehouse with code '{normalizedCode}' already exists.", "WAREHOUSE_CODE_EXISTS", 400);
        }

        var warehouse = new Warehouse(
            normalizedCode,
            request.Name,
            request.Address,
            request.IsActive
        );

        await _context.Warehouses.AddAsync(warehouse, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(warehouse);
    }

    public async Task<WarehouseDto> UpdateWarehouseAsync(Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse == null)
        {
            throw new AppException($"Warehouse with ID '{id}' was not found.", "WAREHOUSE_NOT_FOUND", 404);
        }

        warehouse.Update(request.Name, request.Address, request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(warehouse);
    }

    public async Task DeleteWarehouseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse == null)
        {
            throw new AppException($"Warehouse with ID '{id}' was not found.", "WAREHOUSE_NOT_FOUND", 404);
        }

        // Soft delete/deactivate warehouse
        warehouse.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static WarehouseDto MapToDto(Warehouse w)
    {
        return new WarehouseDto(
            w.Id,
            w.Code,
            w.Name,
            w.Address,
            w.IsActive,
            w.CreatedAt,
            w.UpdatedAt
        );
    }
}
