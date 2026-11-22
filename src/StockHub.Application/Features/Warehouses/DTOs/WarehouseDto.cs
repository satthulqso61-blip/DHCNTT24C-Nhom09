namespace StockHub.Application.Features.Warehouses.DTOs;

public sealed record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record CreateWarehouseRequest(
    string Code,
    string Name,
    string Address,
    bool IsActive = true
);

public sealed record UpdateWarehouseRequest(
    string Name,
    string Address,
    bool IsActive
);
