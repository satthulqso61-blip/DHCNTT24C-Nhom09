using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Features.Warehouses.DTOs;
using StockHub.Application.Features.Warehouses.Services;
using StockHub.Application.Features.Warehouses.Validators;
using StockHub.Domain.Entities;
using StockHub.Infrastructure.Persistence;
using Xunit;

namespace StockHub.UnitTests.Services;

public class WarehouseServiceTests : IDisposable
{
    private readonly StockHubDbContext _dbContext;
    private readonly WarehouseService _sut;

    public WarehouseServiceTests()
    {
        var options = new DbContextOptionsBuilder<StockHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"WarehouseTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new StockHubDbContext(options);
        _sut = new WarehouseService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region CRUD & Business Logic Tests

    [Fact]
    public async Task CreateWarehouseAsync_WithValidRequest_CreatesAndReturnsWarehouse()
    {
        // Arrange
        var request = new CreateWarehouseRequest("WH-HCM", "Kho TP.HCM", "Quận 1, TP.HCM", true);

        // Act
        var result = await _sut.CreateWarehouseAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("WH-HCM");
        result.Name.Should().Be("Kho TP.HCM");
        result.Address.Should().Be("Quận 1, TP.HCM");
        result.IsActive.Should().BeTrue();

        var saved = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Code == "WH-HCM");
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Kho TP.HCM");
    }

    [Fact]
    public async Task CreateWarehouseAsync_WhenCodeAlreadyExists_ThrowsAppExceptionWithWarehouseCodeExists()
    {
        // Arrange
        var existing = new Warehouse("WH-HN", "Kho Ha Noi", "Address 1");
        await _dbContext.Warehouses.AddAsync(existing);
        await _dbContext.SaveChangesAsync();

        var request = new CreateWarehouseRequest("wh-hn", "Kho Ha Noi 2", "Address 2");

        // Act
        var act = () => _sut.CreateWarehouseAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("WAREHOUSE_CODE_EXISTS");
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetWarehouseByIdAsync_WhenExists_ReturnsWarehouseDto()
    {
        // Arrange
        var warehouse = new Warehouse("WH-DN", "Kho Da Nang", "Hai Chau, Da Nang");
        await _dbContext.Warehouses.AddAsync(warehouse);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetWarehouseByIdAsync(warehouse.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(warehouse.Id);
        result.Code.Should().Be("WH-DN");
        result.Name.Should().Be("Kho Da Nang");
    }

    [Fact]
    public async Task GetWarehouseByIdAsync_WhenNotExists_ThrowsAppExceptionWithWarehouseNotFound()
    {
        // Act
        var act = () => _sut.GetWarehouseByIdAsync(Guid.NewGuid());

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("WAREHOUSE_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateWarehouseAsync_WhenExists_UpdatesAndReturnsWarehouseDto()
    {
        // Arrange
        var warehouse = new Warehouse("WH-HP", "Kho Hai Phong Cu", "Le Hong Phong");
        await _dbContext.Warehouses.AddAsync(warehouse);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateWarehouseRequest("Kho Hai Phong Moi", "Dinh Vu, Hai Phong", true);

        // Act
        var result = await _sut.UpdateWarehouseAsync(warehouse.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Kho Hai Phong Moi");
        result.Address.Should().Be("Dinh Vu, Hai Phong");

        var updatedEntity = await _dbContext.Warehouses.FindAsync(warehouse.Id);
        updatedEntity!.Name.Should().Be("Kho Hai Phong Moi");
        updatedEntity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateWarehouseAsync_WhenNotExists_ThrowsAppExceptionWithWarehouseNotFound()
    {
        // Arrange
        var updateRequest = new UpdateWarehouseRequest("Ten Kho", "Dia Chi", true);

        // Act
        var act = () => _sut.UpdateWarehouseAsync(Guid.NewGuid(), updateRequest);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("WAREHOUSE_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteWarehouseAsync_WhenExists_DeactivatesWarehouse()
    {
        // Arrange
        var warehouse = new Warehouse("WH-CT", "Kho Can Tho", "Ninh Kieu", true);
        await _dbContext.Warehouses.AddAsync(warehouse);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteWarehouseAsync(warehouse.Id);

        // Assert
        var entity = await _dbContext.Warehouses.FindAsync(warehouse.Id);
        entity.Should().NotBeNull();
        entity!.IsActive.Should().BeFalse();
        entity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWarehouseAsync_WhenNotExists_ThrowsAppExceptionWithWarehouseNotFound()
    {
        // Act
        var act = () => _sut.DeleteWarehouseAsync(Guid.NewGuid());

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("WAREHOUSE_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetWarehousesAsync_WithSearchAndFilter_ReturnsPagedResult()
    {
        // Arrange
        await _dbContext.Warehouses.AddRangeAsync(
            new Warehouse("WH-01", "Kho Tong Ha Noi", "HN", true),
            new Warehouse("WH-02", "Kho Chi Nhanh Bac Ninh", "BN", true),
            new Warehouse("WH-03", "Kho Cu Khong Dung", "HN", false)
        );
        await _dbContext.SaveChangesAsync();

        // Act - Search "Ha Noi" & only active
        var result = await _sut.GetWarehousesAsync(search: "Ha Noi", isActive: true, page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Code.Should().Be("WH-01");
    }

    [Fact]
    public async Task GetAllActiveWarehousesAsync_ReturnsOnlyActiveWarehouses()
    {
        // Arrange
        await _dbContext.Warehouses.AddRangeAsync(
            new Warehouse("WH-ACTIVE-1", "Kho 1", "Addr", true),
            new Warehouse("WH-INACTIVE", "Kho 2", "Addr", false),
            new Warehouse("WH-ACTIVE-2", "Kho 3", "Addr", true)
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllActiveWarehousesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(w => w.IsActive);
    }

    #endregion

    #region Validator Tests

    [Theory]
    [InlineData("", "Valid Name")]
    [InlineData("W", "Valid Name")]
    [InlineData("TOO_LONG_CODE_EXCEEDING_LIMIT_1234567890", "Valid Name")]
    [InlineData("INVALID CODE!", "Valid Name")]
    [InlineData("WH-01", "")]
    public void CreateWarehouseRequestValidator_WithInvalidInputs_HasValidationErrors(string code, string name)
    {
        // Arrange
        var validator = new CreateWarehouseRequestValidator();
        var request = new CreateWarehouseRequest(code, name, "Valid Address");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateWarehouseRequestValidator_WithEmptyName_HasValidationError()
    {
        // Arrange
        var validator = new UpdateWarehouseRequestValidator();
        var request = new UpdateWarehouseRequest("", "Address", true);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion
}
