using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshAsync(RefreshRequest request, string ipAddress, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    }
}
