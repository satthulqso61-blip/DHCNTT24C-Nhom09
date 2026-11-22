using Microsoft.EntityFrameworkCore;
using StockHub.Domain.Entities;

namespace StockHub.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
