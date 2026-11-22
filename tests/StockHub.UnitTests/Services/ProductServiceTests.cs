using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Features.Products.DTOs;
using StockHub.Application.Features.Products.Services;
using StockHub.Application.Features.Products.Validators;
using StockHub.Domain.Entities;
using StockHub.Infrastructure.Persistence;
using Xunit;

namespace StockHub.UnitTests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly StockHubDbContext _dbContext;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<StockHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProductTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new StockHubDbContext(options);
        _sut = new ProductService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region CRUD & Soft Delete Tests

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_CreatesAndReturnsProduct()
    {
        // Arrange
        var request = new CreateProductRequest("LAP-DELL-7420", "Laptop Dell Latitude 7420", "Chiếc", 22000000m);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("LAP-DELL-7420");
        result.Name.Should().Be("Laptop Dell Latitude 7420");
        result.Unit.Should().Be("Chiếc");
        result.BasePrice.Should().Be(22000000m);
        result.IsDeleted.Should().BeFalse();

        var saved = await _dbContext.Products.FirstOrDefaultAsync(p => p.SKU == "LAP-DELL-7420");
        saved.Should().NotBeNull();
        saved!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateProductAsync_WhenSkuAlreadyExists_ThrowsAppExceptionWithProductSkuExists()
    {
        // Arrange
        var existing = new Product("SKU-EXISTING", "Old Product", "Cái", 100000m);
        await _dbContext.Products.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var request = new CreateProductRequest("sku-existing", "New Product", "Cái", 150000m);

        // Act
        var act = () => _sut.CreateProductAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("PRODUCT_SKU_EXISTS");
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenActiveProductExists_ReturnsProductDto()
    {
        // Arrange
        var product = new Product("PROD-01", "Product One", "Hop", 50000m);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetProductByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.SKU.Should().Be("PROD-01");
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductIsSoftDeleted_ThrowsAppExceptionWithProductNotFound()
    {
        // Arrange
        var product = new Product("PROD-DEL", "Deleted Product", "Cái", 50000m);
        product.SoftDelete();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => _sut.GetProductByIdAsync(product.Id);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductNotExists_ThrowsAppExceptionWithProductNotFound()
    {
        // Act
        var act = () => _sut.GetProductByIdAsync(Guid.NewGuid());

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenExists_UpdatesAndReturnsProductDto()
    {
        // Arrange
        var product = new Product("PROD-UPDATE", "Initial Name", "Cái", 100000m);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateProductRequest("Updated Name", "Thung", 120000m);

        // Act
        var result = await _sut.UpdateProductAsync(product.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Unit.Should().Be("Thung");
        result.BasePrice.Should().Be(120000m);
        result.UpdatedAt.Should().NotBeNull();

        var updated = await _dbContext.Products.FindAsync(product.Id);
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductIsDeleted_ThrowsAppExceptionWithProductNotFound()
    {
        // Arrange
        var product = new Product("PROD-DEL-UPD", "Old Product", "Cái", 100000m);
        product.SoftDelete();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateProductRequest("Try Update", "Cái", 120000m);

        // Act
        var act = () => _sut.UpdateProductAsync(product.Id, request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteProductAsync_SoftDeletesProduct_AndDoesNotHardDeleteFromDb()
    {
        // Arrange
        var product = new Product("PROD-SOFT-DEL", "Soft Delete Candidate", "Cái", 75000m);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act - Soft Delete
        await _sut.DeleteProductAsync(product.Id);

        // Assert
        // Verify entity still exists in Database (NO HARD DELETE)
        var rawEntity = await _dbContext.Products.FindAsync(product.Id);
        rawEntity.Should().NotBeNull();
        rawEntity!.IsDeleted.Should().BeTrue();
        rawEntity.UpdatedAt.Should().NotBeNull();

        // Verify standard active queries exclude it
        var activeList = await _sut.GetAllProductsAsync();
        activeList.Should().NotContain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task RestoreProductAsync_WhenProductWasDeleted_RestoresProduct()
    {
        // Arrange
        var product = new Product("PROD-RESTORE", "Restore Candidate", "Cái", 75000m);
        product.SoftDelete();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var restored = await _sut.RestoreProductAsync(product.Id);

        // Assert
        restored.Should().NotBeNull();
        restored.IsDeleted.Should().BeFalse();

        var entity = await _dbContext.Products.FindAsync(product.Id);
        entity!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreProductAsync_WhenProductIsNotDeleted_ThrowsAppExceptionWithProductNotFound()
    {
        // Arrange
        var product = new Product("PROD-NOT-DEL", "Not Deleted", "Cái", 75000m);
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => _sut.RestoreProductAsync(product.Id);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("PRODUCT_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProductsAsync_WithSearchAndPagination_ReturnsExpectedResults()
    {
        // Arrange
        await _dbContext.Products.AddRangeAsync(
            new Product("SKU-LAP-1", "Laptop Dell Latitude", "Chiếc", 20000000m),
            new Product("SKU-LAP-2", "Laptop Lenovo ThinkPad", "Chiếc", 22000000m),
            new Product("SKU-MOU-1", "Mouse Logitech", "Cái", 500000m)
        );
        var deletedProd = new Product("SKU-LAP-DEL", "Laptop cu da xoa", "Chiếc", 10000000m);
        deletedProd.SoftDelete();
        await _dbContext.Products.AddAsync(deletedProd);
        await _dbContext.SaveChangesAsync();

        // Act - Search "Laptop" without deleted
        var activeLaptops = await _sut.GetProductsAsync(search: "Laptop", page: 1, pageSize: 10, includeDeleted: false);

        // Assert
        activeLaptops.TotalCount.Should().Be(2);
        activeLaptops.Items.Should().HaveCount(2);
        activeLaptops.Items.Should().OnlyContain(p => !p.IsDeleted);

        // Act - Search "Laptop" including deleted
        var allLaptops = await _sut.GetProductsAsync(search: "Laptop", page: 1, pageSize: 10, includeDeleted: true);
        allLaptops.TotalCount.Should().Be(3);
    }

    #endregion

    #region Validator Tests

    [Theory]
    [InlineData("", "Valid Name", "Unit", 100)]
    [InlineData("SK", "Valid Name", "Unit", 100)] // Too short (< 3)
    [InlineData("INVALID SKU!", "Valid Name", "Unit", 100)] // Special characters
    [InlineData("SKU-01", "", "Unit", 100)] // Empty name
    [InlineData("SKU-01", "Valid Name", "", 100)] // Empty unit
    [InlineData("SKU-01", "Valid Name", "Unit", -10)] // Negative price
    public void CreateProductRequestValidator_WithInvalidInputs_HasValidationErrors(string sku, string name, string unit, decimal price)
    {
        // Arrange
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest(sku, name, unit, price);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "Unit", 100)]
    [InlineData("Valid Name", "", 100)]
    [InlineData("Valid Name", "Unit", -1)]
    public void UpdateProductRequestValidator_WithInvalidInputs_HasValidationErrors(string name, string unit, decimal price)
    {
        // Arrange
        var validator = new UpdateProductRequestValidator();
        var request = new UpdateProductRequest(name, unit, price);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion
}
