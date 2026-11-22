using MediatR;
using StockHub.Application.Features.Auth.DTOs;
using StockHub.Application.Features.Auth.Services;

namespace StockHub.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(
    Guid UserId
) : IRequest<UserDto>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IAuthService _authService;

    public GetCurrentUserQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetCurrentUserAsync(request.UserId, cancellationToken);
    }
}
