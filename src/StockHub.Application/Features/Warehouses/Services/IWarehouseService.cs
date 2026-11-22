using StockHub.Application.Common.Models;
using StockHub.Application.Features.Warehouses.DTOs;

namespace StockHub.Application.Features.Warehouses.Services;

public interface IWarehouseService
{
    Task<PagedResult<WarehouseDto>> GetWarehousesAsync(
        string? search,
        bool? isActive,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WarehouseDto>> GetAllActiveWarehousesAsync(CancellationToken cancellationToken = default);

    Task<WarehouseDto> GetWarehouseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);

    Task<WarehouseDto> UpdateWarehouseAsync(Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken = default);

    Task DeleteWarehouseAsync(Guid id, CancellationToken cancellationToken = default);
}
