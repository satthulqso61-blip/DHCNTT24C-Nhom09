using StockHub.Domain.Enums;

namespace StockHub.Application.Features.Auth.DTOs;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive
);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User
);

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    UserRole Role
);

public sealed record LoginRequest(
    string UsernameOrEmail,
    string Password
);

public sealed record RefreshTokenRequest(
    string RefreshToken
);
