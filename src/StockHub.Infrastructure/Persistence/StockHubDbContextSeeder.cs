using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Interfaces;
using StockHub.Domain.Entities;
using StockHub.Domain.Enums;

namespace StockHub.Infrastructure.Persistence;

public static class StockHubDbContextSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Admin
        var adminExists = await context.Users.AnyAsync(u => u.Username == "admin");
        if (!adminExists)
        {
            var adminUser = new User(
                username: "admin",
                email: "admin@stockhub.local",
                passwordHash: passwordHasher.HashPassword("admin"),
                fullName: "System Administrator",
                role: UserRole.Admin
            );
            await context.Users.AddAsync(adminUser);
        }

        // 2. Seed WarehouseStaff
        var staffExists = await context.Users.AnyAsync(u => u.Username == "staff");
        if (!staffExists)
        {
            var staffUser = new User(
                username: "staff",
                email: "staff@stockhub.local",
                passwordHash: passwordHasher.HashPassword("staff"),
                fullName: "Warehouse Staff",
                role: UserRole.WarehouseStaff
            );
            await context.Users.AddAsync(staffUser);
        }

        // 3. Seed Accountant
        var accountantExists = await context.Users.AnyAsync(u => u.Username == "accountant");
        if (!accountantExists)
        {
            var accountantUser = new User(
                username: "accountant",
                email: "accountant@stockhub.local",
                passwordHash: passwordHasher.HashPassword("accountant"),
                fullName: "Chief Accountant",
                role: UserRole.Accountant
            );
            await context.Users.AddAsync(accountantUser);
        }

        await context.SaveChangesAsync();
    }
}
