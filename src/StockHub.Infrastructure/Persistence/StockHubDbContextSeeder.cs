using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Interfaces;
using StockHub.Domain.Entities;
using StockHub.Domain.Enums;

namespace StockHub.Infrastructure.Persistence;

public static class StockHubDbContextSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Seed Accounts (Admin, WarehouseStaff, Accountant)
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

        // 2. Seed 3 Default Warehouses
        var warehousesExist = await context.Warehouses.AnyAsync();
        if (!warehousesExist)
        {
            var defaultWarehouses = new List<Warehouse>
            {
                new("WH-CENTRAL", "Kho Tổng (Hà Nội)", "123 Đường Giải Phóng, Hai Bà Trưng, Hà Nội", true),
                new("WH-NORTH", "Kho Miền Bắc (Hải Phòng)", "45 Đường Lê Hồng Phong, Ngô Quyền, Hải Phòng", true),
                new("WH-SOUTH", "Kho Miền Nam (TP. Hồ Chí Minh)", "789 Đường Nguyễn Văn Linh, Quận 7, TP. Hồ Chí Minh", true)
            };

            await context.Warehouses.AddRangeAsync(defaultWarehouses);
        }

        // 3. Seed 10 Default Products (SKUs)
        var productsExist = await context.Products.AnyAsync();
        if (!productsExist)
        {
            var defaultProducts = new List<Product>
            {
                new("LAP-DELL-5520", "Laptop Dell Latitude 5520 i7 16GB", "Chiếc", 24500000m),
                new("LAP-THINK-T14", "Laptop ThinkPad T14 Gen 4 Ryzen 7", "Chiếc", 26000000m),
                new("MNTR-DELL-24", "Màn hình Dell UltraSharp U2422H 24 inch", "Chiếc", 5800000m),
                new("MNTR-LG-27", "Màn hình LG 27UP850 27 inch 4K HDR", "Chiếc", 8900000m),
                new("RAM-DDR4-16G", "RAM Kingston Fury Beast 16GB DDR4 3200MHz", "Thanh", 1150000m),
                new("RAM-DDR5-32G", "RAM Corsair Vengeance 32GB DDR5 5600MHz", "Thanh", 2800000m),
                new("SSD-NVME-1TB", "Ổ cứng SSD Samsung 980 PRO 1TB PCIe 4.0 NVMe", "Cái", 2450000m),
                new("SSD-NVME-500", "Ổ cứng SSD Kingston NV2 500GB PCIe 4.0 NVMe", "Cái", 1100000m),
                new("KEY-MECH-RGB", "Bàn phím cơ không dây Logitech MX Mechanical", "Cái", 3200000m),
                new("MOU-LOGI-MX3", "Chuột không dây công thái học Logitech MX Master 3S", "Cái", 2100000m)
            };

            await context.Products.AddRangeAsync(defaultProducts);
        }

        await context.SaveChangesAsync();
    }
}
