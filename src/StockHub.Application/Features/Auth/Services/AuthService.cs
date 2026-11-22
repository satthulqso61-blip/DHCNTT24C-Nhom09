using Microsoft.EntityFrameworkCore;
using StockHub.Application.Common.Exceptions;
using StockHub.Application.Common.Interfaces;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Domain.Entities;

namespace StockHub.Application.Features.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == normalizedUsername, cancellationToken);
        if (usernameExists)
        {
            throw new AppException($"Username '{request.Username}' is already taken.", "USERNAME_TAKEN", 400);
        }

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new AppException($"Email '{request.Email}' is already registered.", "EMAIL_ALREADY_EXISTS", 400);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(
            normalizedUsername,
            normalizedEmail,
            passwordHash,
            request.FullName,
            request.Role
        );

        var (refreshTokenString, refreshTokenExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenEntity = user.AddRefreshToken(refreshTokenString, refreshTokenExpiresAt);

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponse(
            accessToken,
            refreshTokenString,
            900, // 15 minutes = 900s
            MapToDto(user)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var identifier = request.UsernameOrEmail.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new AppException("Invalid username/email or password.", "INVALID_CREDENTIALS", 401);
        }

        var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new AppException("Invalid username/email or password.", "INVALID_CREDENTIALS", 401);
        }

        var (refreshTokenString, refreshTokenExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenEntity = user.AddRefreshToken(refreshTokenString, refreshTokenExpiresAt);
        await _context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponse(
            accessToken,
            refreshTokenString,
            900,
            MapToDto(user)
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AppException("Refresh token cannot be empty.", "INVALID_REFRESH_TOKEN", 400);
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new AppException("Invalid refresh token.", "INVALID_REFRESH_TOKEN", 401);
        }

        var existingToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (existingToken == null)
        {
            throw new AppException("Invalid refresh token.", "INVALID_REFRESH_TOKEN", 401);
        }

        if (existingToken.IsRevoked)
        {
            throw new AppException("Refresh token has been revoked.", "TOKEN_REVOKED", 401);
        }

        if (existingToken.IsExpired)
        {
            throw new AppException("Refresh token has expired. Please login again.", "TOKEN_EXPIRED", 401);
        }

        // Token rotation: generate new refresh token, revoke the old one
        var (newRefreshTokenString, newRefreshTokenExpiresAt) = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.Revoke(newRefreshTokenString);
        var newRefreshTokenEntity = user.AddRefreshToken(newRefreshTokenString, newRefreshTokenExpiresAt);
        await _context.RefreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponse(
            newAccessToken,
            newRefreshTokenString,
            900,
            MapToDto(user)
        );
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new AppException("User not found.", "USER_NOT_FOUND", 404);
        }

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role,
            user.IsActive
        );
    }
}
