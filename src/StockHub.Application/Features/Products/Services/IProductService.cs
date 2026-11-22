using StockHub.Application.Common.Models;
using StockHub.Application.Features.Products.DTOs;

namespace StockHub.Application.Features.Products.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(
        string? search,
        int page = 1,
        int pageSize = 10,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);

    Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default);
}
