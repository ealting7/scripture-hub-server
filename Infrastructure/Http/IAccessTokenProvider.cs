using System.Threading;

namespace scripture_hub_server.Infrastructure.Http;

// Application must implement this to provide access tokens and refresh when needed.
public interface IAccessTokenProvider
{
    // Get current access token (may be from cache)
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    // Force refresh token and return new token
    Task<string?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);
}
