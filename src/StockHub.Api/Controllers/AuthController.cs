using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockHub.Application.Features.Auth.Commands.Login;
using StockHub.Application.Features.Auth.Commands.RefreshToken;
using StockHub.Application.Features.Auth.Commands.Register;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Application.Features.Auth.Queries.GetCurrentUser;

namespace StockHub.Api.Controllers;

public sealed class AuthController : ApiControllerBase
{
    /// <summary>
    /// Register a new user account (Admin, WarehouseStaff, or Accountant).
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var response = await Sender.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Authenticate user credentials and issue Access Token and Refresh Token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var response = await Sender.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Refresh an expired Access Token using a valid Refresh Token (Token Rotation).
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var response = await Sender.Send(command, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get currently authenticated user profile and roles.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var response = await Sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(response);
    }
}
