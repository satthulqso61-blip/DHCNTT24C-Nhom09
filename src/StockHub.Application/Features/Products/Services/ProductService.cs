using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Common.Interfaces;
using StockHub.Application.Common.Models;
using StockHub.Application.Features.Products.DTOs;
using StockHub.Domain.Entities;

namespace StockHub.Application.Features.Products.Services;

public sealed class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        string? search,
        int page = 1,
        int pageSize = 10,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.SKU.ToLower().Contains(searchLower) || p.Name.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.SKU)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.SKU)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new AppException($"Product with ID '{id}' was not found.", "PRODUCT_NOT_FOUND", 404);
        }

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedSku = request.SKU.Trim().ToUpperInvariant();

        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == normalizedSku && !p.IsDeleted, cancellationToken);

        if (skuExists)
        {
            throw new AppException($"Product with SKU '{normalizedSku}' already exists.", "PRODUCT_SKU_EXISTS", 400);
        }

        var product = new Product(
            normalizedSku,
            request.Name,
            request.Unit,
            request.BasePrice
        );

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new AppException($"Product with ID '{id}' was not found.", "PRODUCT_NOT_FOUND", 404);
        }

        product.Update(request.Name, request.Unit, request.BasePrice);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new AppException($"Product with ID '{id}' was not found.", "PRODUCT_NOT_FOUND", 404);
        }

        // Soft delete: sets IsDeleted = true
        product.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDto> RestoreProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted, cancellationToken);

        if (product == null)
        {
            throw new AppException($"Deleted product with ID '{id}' was not found.", "PRODUCT_NOT_FOUND", 404);
        }

        product.Restore();
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto(
            p.Id,
            p.SKU,
            p.Name,
            p.Unit,
            p.BasePrice,
            p.IsDeleted,
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
