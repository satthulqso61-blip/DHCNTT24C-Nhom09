using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StockHub.Domain.Entities;
using StockHub.Domain.Enums;
using StockHub.Infrastructure.Services;
using Xunit;

namespace StockHub.UnitTests.Services;

public class JwtTokenGeneratorTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenGenerator _sut;

    public JwtTokenGeneratorTests()
    {
        _jwtOptions = new JwtOptions
        {
            SecretKey = "SuperSecretKeyForTestingPurposesMustBeLongEnough1234567890",
            Issuer = "StockHub.Api.Test",
            Audience = "StockHub.Client.Test",
            ExpiryMinutes = 30,
            RefreshTokenExpiryDays = 14
        };

        var optionsWrapper = Options.Create(_jwtOptions);
        _sut = new JwtTokenGenerator(optionsWrapper);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsValidJwtStringWithCorrectClaims()
    {
        // Arrange
        var user = new User("admin_test", "admin@stockhub.local", "hashed_pwd", "Admin Test", UserRole.Admin);

        // Act
        var tokenString = _sut.GenerateAccessToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        jwt.Issuer.Should().Be(_jwtOptions.Issuer);
        jwt.Audiences.Should().Contain(_jwtOptions.Audience);

        var claims = jwt.Claims.ToList();
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        claims.Should().Contain(c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == UserRole.Admin.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUrlSafeTokenAndFutureExpiry()
    {
        // Act
        var (token, expiresAt) = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().NotContain("+").And.NotContain("/").And.NotEndWith("=");
        expiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(13));
    }
}
