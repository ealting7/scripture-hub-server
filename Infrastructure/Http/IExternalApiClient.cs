using System.Net.Http;

namespace scripture_hub_server.Infrastructure.Http;

public interface IExternalApiClient
{
    Task<HttpResponseMessage> GetAsync(string relativeUrl, CancellationToken cancellationToken = default);
}
