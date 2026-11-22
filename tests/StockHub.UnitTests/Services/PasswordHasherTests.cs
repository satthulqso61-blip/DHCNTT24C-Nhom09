using FluentAssertions;
using StockHub.Infrastructure.Services;
using Xunit;

namespace StockHub.UnitTests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyHashDifferentFromPassword()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "CorrectPassword!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var password = "CorrectPassword!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword("WrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("password", "")]
    [InlineData("   ", "   ")]
    public void VerifyPassword_WithEmptyOrNull_ReturnsFalse(string password, string hash)
    {
        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeFalse();
    }
}
