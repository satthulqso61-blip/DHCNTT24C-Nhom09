namespace StockHub.Application.Features.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string SKU,
    string Name,
    string Unit,
    decimal BasePrice,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record CreateProductRequest(
    string SKU,
    string Name,
    string Unit,
    decimal BasePrice
);

public sealed record UpdateProductRequest(
    string Name,
    string Unit,
    decimal BasePrice
);
