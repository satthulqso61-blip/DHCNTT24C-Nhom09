using FluentAssertions;
using Moq;
using StockHub.Application.Features.Auth.Commands.Login;
using StockHub.Application.Features.Auth.Commands.RefreshToken;
using StockHub.Application.Features.Auth.Commands.Register;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Application.Features.Auth.Queries.GetCurrentUser;
using StockHub.Application.Features.Auth.Services;
using StockHub.Domain.Enums;
using Xunit;

namespace StockHub.UnitTests.Features.Auth;

public class AuthCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    #region Register Command & Validator

    [Fact]
    public async Task RegisterCommandHandler_CallsAuthService_ReturnsAuthResponse()
    {
        // Arrange
        var command = new RegisterCommand("newuser", "user@test.com", "Password123!", "New User", UserRole.WarehouseStaff);
        var expectedResponse = new AuthResponse("access_token", "refresh_token", 900, new UserDto(Guid.NewGuid(), "newuser", "user@test.com", "New User", UserRole.WarehouseStaff, true));

        _authServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new RegisterCommandHandler(_authServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _authServiceMock.Verify(s => s.RegisterAsync(It.Is<RegisterRequest>(r => r.Username == command.Username), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "valid@email.com", "ValidPass123!")]
    [InlineData("us", "valid@email.com", "ValidPass123!")]
    [InlineData("validuser", "invalid-email", "ValidPass123!")]
    [InlineData("validuser", "valid@email.com", "short")]
    public void RegisterCommandValidator_WithInvalidInputs_HasValidationErrors(string username, string email, string password)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand(username, email, password, "Full Name", UserRole.Admin);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Login Command & Validator

    [Fact]
    public async Task LoginCommandHandler_CallsAuthService_ReturnsAuthResponse()
    {
        // Arrange
        var command = new LoginCommand("admin", "adminpass");
        var expectedResponse = new AuthResponse("access_token", "refresh_token", 900, new UserDto(Guid.NewGuid(), "admin", "admin@stockhub.local", "Admin", UserRole.Admin, true));

        _authServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new LoginCommandHandler(_authServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _authServiceMock.Verify(s => s.LoginAsync(It.Is<LoginRequest>(r => r.UsernameOrEmail == "admin"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("username", "")]
    public void LoginCommandValidator_WithEmptyFields_HasValidationErrors(string usernameOrEmail, string password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(usernameOrEmail, password);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Refresh Token Command & Validator

    [Fact]
    public async Task RefreshTokenCommandHandler_CallsAuthService_ReturnsAuthResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid_refresh_token_123");
        var expectedResponse = new AuthResponse("new_access_token", "new_refresh_token", 900, new UserDto(Guid.NewGuid(), "user", "u@test.com", "User", UserRole.Accountant, true));

        _authServiceMock
            .Setup(s => s.RefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new RefreshTokenCommandHandler(_authServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public void RefreshTokenCommandValidator_WithEmptyToken_HasValidationError()
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Get Current User Query

    [Fact]
    public async Task GetCurrentUserQueryHandler_CallsAuthService_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery(userId);
        var expectedUser = new UserDto(userId, "staff", "staff@stockhub.local", "Staff", UserRole.WarehouseStaff, true);

        _authServiceMock
            .Setup(s => s.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        var handler = new GetCurrentUserQueryHandler(_authServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
    }

    #endregion
}
