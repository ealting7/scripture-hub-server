using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Application.Interfaces;

public interface IJwtTokenService
{
    //Task<AuthResponse> GenerateTokensAsync(AppUser user, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> GenerateTokensAsync(UserIdentity user, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokensAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
}
