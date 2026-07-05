using System.Net.Http.Json;

namespace scripture_hub_server.Infrastructure.Http;

public class ExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ExternalApiClient>? _logger;

    public ExternalApiClient(HttpClient http, ILogger<ExternalApiClient>? logger = null)
    {
        _http = http;
        _logger = logger;
    }

    public Task<HttpResponseMessage> GetAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        return _http.GetAsync(relativeUrl, cancellationToken);
    }
}
