using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockHub.Domain.Entities;

namespace StockHub.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Address)
            .HasMaxLength(250);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();
    }
}
