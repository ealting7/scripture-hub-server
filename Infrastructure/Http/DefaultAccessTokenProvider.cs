namespace scripture_hub_server.Infrastructure.Http;

/// <summary>
/// Default implementation of IAccessTokenProvider that returns null tokens.
/// This is a placeholder for applications that don't use client-side token refresh.
/// Override or replace with a custom implementation when token refresh is needed.
/// </summary>
public class DefaultAccessTokenProvider : IAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // No token available from this provider
        return Task.FromResult<string?>(null);
    }

    public Task<string?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // No token refresh capability
        return Task.FromResult<string?>(null);
    }
}
