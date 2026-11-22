using FluentValidation;
using MediatR;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Application.Features.Auth.Services;

namespace StockHub.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string UsernameOrEmail,
    string Password
) : IRequest<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(new LoginRequest(
            request.UsernameOrEmail,
            request.Password
        ), cancellationToken);
    }
}
