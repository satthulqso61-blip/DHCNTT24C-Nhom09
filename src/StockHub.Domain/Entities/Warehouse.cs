using StockHub.Domain.Common;

namespace StockHub.Domain.Entities;

public sealed class Warehouse : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Warehouse() { } // EF Core

    public Warehouse(string code, string name, string address, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code cannot be empty.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty.", nameof(name));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Address = address?.Trim() ?? string.Empty;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string address, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name cannot be empty.", nameof(name));

        Name = name.Trim();
        Address = address?.Trim() ?? string.Empty;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
