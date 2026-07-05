using System.Net.Http.Headers;
using System.Text;

namespace scripture_hub_server.Infrastructure.Http;

// DelegatingHandler that attaches access token and coordinates token refresh on 401 responses.
public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly TokenRefreshLock _lock;
    private readonly ILogger<TokenRefreshHandler>? _logger;

    public TokenRefreshHandler(IAccessTokenProvider tokenProvider, TokenRefreshLock tokenRefreshLock, ILogger<TokenRefreshHandler>? logger = null)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _lock = tokenRefreshLock ?? throw new ArgumentNullException(nameof(tokenRefreshLock));
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attach token
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger?.LogInformation("Received 401. Attempting coordinated token refresh.");

            // Only one caller refreshes token while others wait
            using (await _lock.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                // Attempt to refresh token
                var newToken = await _tokenProvider.RefreshAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    // Dispose previous response and retry once
                    response.Dispose();

                    // Clone request for retry
                    var newRequest = await CloneHttpRequestMessageAsync(request).ConfigureAwait(false);
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                    return await base.SendAsync(newRequest, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // Copy request content (if any)
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            // Copy content headers
            if (request.Content.Headers != null)
            {
                foreach (var h in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
        }

        // Copy the rest of the request
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        clone.Version = request.Version;
        return clone;
    }
}
