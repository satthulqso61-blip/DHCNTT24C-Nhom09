using StockHub.Domain.Common;

namespace StockHub.Domain.Entities;

public sealed class Product : Entity
{
    public string SKU { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal BasePrice { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { } // EF Core

    public Product(string sku, string name, string unit, decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Product SKU cannot be empty.", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Product unit cannot be empty.", nameof(unit));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        SKU = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Unit = unit.Trim();
        BasePrice = basePrice;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string unit, decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Product unit cannot be empty.", nameof(unit));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        Name = name.Trim();
        Unit = unit.Trim();
        BasePrice = basePrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
