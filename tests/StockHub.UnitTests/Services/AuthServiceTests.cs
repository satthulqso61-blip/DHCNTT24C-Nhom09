using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Common.Interfaces;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Application.Features.Auth.Services;
using StockHub.Domain.Entities;
using StockHub.Domain.Enums;
using StockHub.Infrastructure.Persistence;
using StockHub.Infrastructure.Services;
using Xunit;

namespace StockHub.UnitTests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly StockHubDbContext _dbContext;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly AuthService _sut; // System Under Test

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<StockHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"StockHubTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new StockHubDbContext(options);

        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        // Default mock behaviors
        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns<string>(p => $"hashed_{p}");

        _passwordHasherMock
            .Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((p, h) => h == $"hashed_{p}");

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateAccessToken(It.IsAny<User>()))
            .Returns("valid_jwt_access_token");

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateRefreshToken())
            .Returns(() => ($"refresh_token_{Guid.NewGuid()}", DateTime.UtcNow.AddDays(7)));

        _sut = new AuthService(_dbContext, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region 1. Register Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponseAndCreatesUser()
    {
        // Arrange
        var request = new RegisterRequest(
            Username: "newuser",
            Email: "newuser@stockhub.local",
            Password: "SecurePassword123!",
            FullName: "New Operator",
            Role: UserRole.WarehouseStaff
        );

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_jwt_access_token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be("newuser");
        result.User.Email.Should().Be("newuser@stockhub.local");
        result.User.Role.Should().Be(UserRole.WarehouseStaff);
        result.User.IsActive.Should().BeTrue();

        var savedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Username == "newuser");

        savedUser.Should().NotBeNull();
        savedUser!.PasswordHash.Should().Be("hashed_SecurePassword123!");
        savedUser.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ThrowsAppExceptionWithUsernameTaken()
    {
        // Arrange
        var existingUser = new User("existinguser", "first@stockhub.local", "hash123", "User One", UserRole.Admin);
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest(
            Username: "ExistingUser", // Case-insensitive test
            Email: "different@stockhub.local",
            Password: "Password123!",
            FullName: "User Two",
            Role: UserRole.WarehouseStaff
        );

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("USERNAME_TAKEN");
        ex.Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsAppExceptionWithEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User("firstuser", "duplicate@stockhub.local", "hash123", "User One", UserRole.Admin);
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest(
            Username: "seconduser",
            Email: "DUPLICATE@stockhub.local", // Case-insensitive email test
            Password: "Password123!",
            FullName: "User Two",
            Role: UserRole.WarehouseStaff
        );

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("EMAIL_ALREADY_EXISTS");
        ex.Which.StatusCode.Should().Be(400);
    }

    #endregion

    #region 2. Login Tests

    [Fact]
    public async Task LoginAsync_WithValidUsernameAndPassword_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User("stockstaff", "staff@stockhub.local", "hashed_Secret123", "Staff Member", UserRole.WarehouseStaff);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest(
            UsernameOrEmail: "stockstaff",
            Password: "Secret123"
        );

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_jwt_access_token");
        result.User.Username.Should().Be("stockstaff");
        result.User.Role.Should().Be(UserRole.WarehouseStaff);
    }

    [Fact]
    public async Task LoginAsync_WithValidEmailAndPassword_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User("accountant1", "accountant@stockhub.local", "hashed_AccountantPass", "Senior Accountant", UserRole.Accountant);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest(
            UsernameOrEmail: "accountant@stockhub.local",
            Password: "AccountantPass"
        );

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_jwt_access_token");
        result.User.Email.Should().Be("accountant@stockhub.local");
        result.User.Role.Should().Be(UserRole.Accountant);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsAppExceptionWithInvalidCredentials()
    {
        // Arrange
        var user = new User("adminuser", "admin@stockhub.local", "hashed_CorrectPassword", "Administrator", UserRole.Admin);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest(
            UsernameOrEmail: "adminuser",
            Password: "WrongPassword"
        );

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("INVALID_CREDENTIALS");
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ThrowsAppExceptionWithInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest(
            UsernameOrEmail: "nobody",
            Password: "Password123"
        );

        // Act
        var act = () => _sut.LoginAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("INVALID_CREDENTIALS");
        ex.Which.StatusCode.Should().Be(401);
    }

    #endregion

    #region 3. Refresh Token Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsNewTokensAndRotatesTokens()
    {
        // Arrange
        var user = new User("user1", "user1@stockhub.local", "hashed_pass", "User One", UserRole.WarehouseStaff);
        var oldRefreshToken = "initial_refresh_token_string";
        user.AddRefreshToken(oldRefreshToken, DateTime.UtcNow.AddDays(7));
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.RefreshTokenAsync(oldRefreshToken);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_jwt_access_token");
        result.RefreshToken.Should().NotBe(oldRefreshToken);

        // Verify old token is revoked
        var refreshedUser = await _dbContext.Users
            .Include(u => u.RefreshTokens)
            .FirstAsync(u => u.Id == user.Id);

        var oldTokenEntity = refreshedUser.RefreshTokens.First(rt => rt.Token == oldRefreshToken);
        oldTokenEntity.IsRevoked.Should().BeTrue();
        oldTokenEntity.ReplacedByToken.Should().Be(result.RefreshToken);

        var newTokenEntity = refreshedUser.RefreshTokens.First(rt => rt.Token == result.RefreshToken);
        newTokenEntity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ThrowsAppExceptionWithTokenRevoked()
    {
        // Arrange
        var user = new User("user2", "user2@stockhub.local", "hashed_pass", "User Two", UserRole.WarehouseStaff);
        var revokedToken = "revoked_token_123";
        user.AddRefreshToken(revokedToken, DateTime.UtcNow.AddDays(7));
        user.RevokeRefreshToken(revokedToken, "replacement_token");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => _sut.RefreshTokenAsync(revokedToken);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("TOKEN_REVOKED");
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsAppExceptionWithTokenExpired()
    {
        // Arrange
        var user = new User("user3", "user3@stockhub.local", "hashed_pass", "User Three", UserRole.Accountant);
        var expiredToken = "expired_token_123";
        user.AddRefreshToken(expiredToken, DateTime.UtcNow.AddMinutes(-10)); // Expired in the past
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => _sut.RefreshTokenAsync(expiredToken);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("TOKEN_EXPIRED");
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentToken_ThrowsAppExceptionWithInvalidToken()
    {
        // Act
        var act = () => _sut.RefreshTokenAsync("completely_fake_token");

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("INVALID_REFRESH_TOKEN");
        ex.Which.StatusCode.Should().Be(401);
    }

    #endregion

    #region 4. Get Current User Tests

    [Fact]
    public async Task GetCurrentUserAsync_WithExistingUserId_ReturnsUserDto()
    {
        // Arrange
        var user = new User("accountant_boss", "boss@stockhub.local", "hashed_pass", "Chief Accountant", UserRole.Accountant);
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetCurrentUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Username.Should().Be("accountant_boss");
        result.Email.Should().Be("boss@stockhub.local");
        result.Role.Should().Be(UserRole.Accountant);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithNonExistentUserId_ThrowsAppExceptionWithUserNotFound()
    {
        // Act
        var act = () => _sut.GetCurrentUserAsync(Guid.NewGuid());

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ErrorCode.Should().Be("USER_NOT_FOUND");
        ex.Which.StatusCode.Should().Be(404);
    }

    #endregion

    #region 5. Seeder Tests (Admin, WarehouseStaff, Accountant)

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_SeedsDefault3RolesAndAccounts()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();

        // Act
        await StockHubDbContextSeeder.SeedAsync(_dbContext, passwordHasher);

        // Assert
        var users = await _dbContext.Users.ToListAsync();
        users.Should().HaveCount(3);

        var admin = users.FirstOrDefault(u => u.Username == "admin");
        admin.Should().NotBeNull();
        admin!.Role.Should().Be(UserRole.Admin);
        admin.Email.Should().Be("admin@stockhub.local");
        passwordHasher.VerifyPassword("admin", admin.PasswordHash).Should().BeTrue();

        var staff = users.FirstOrDefault(u => u.Username == "staff");
        staff.Should().NotBeNull();
        staff!.Role.Should().Be(UserRole.WarehouseStaff);
        staff.Email.Should().Be("staff@stockhub.local");
        passwordHasher.VerifyPassword("staff", staff.PasswordHash).Should().BeTrue();

        var accountant = users.FirstOrDefault(u => u.Username == "accountant");
        accountant.Should().NotBeNull();
        accountant!.Role.Should().Be(UserRole.Accountant);
        accountant.Email.Should().Be("accountant@stockhub.local");
        passwordHasher.VerifyPassword("accountant", accountant.PasswordHash).Should().BeTrue();

        // Verify Warehouses seeded (3 warehouses)
        var warehouses = await _dbContext.Warehouses.ToListAsync();
        warehouses.Should().HaveCount(3);
        warehouses.Select(w => w.Code).Should().Contain(new[] { "WH-CENTRAL", "WH-NORTH", "WH-SOUTH" });

        // Verify Products seeded (10 sample SKUs)
        var products = await _dbContext.Products.ToListAsync();
        products.Should().HaveCount(10);
        products.Should().OnlyContain(p => !p.IsDeleted);
    }

    #endregion
}
