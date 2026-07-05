using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Headers;
using scripture_hub_server.Infrastructure.Http;

namespace scripture_hub_server.Infrastructure.Http;

public static class HttpClientDependency
{
    // Configure a standard HttpClient pipeline with common policies.
    // - Retry for transient failures and 5xx
    // - Delegating handler to refresh tokens on 401 with a coordinated lock
    // - Circuit breaker for repeated failures
    // - Timeout policy
    // - Per-client tuning via configuration (appsettings)

    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the token refresh handler and token provider abstraction (app must implement IAccessTokenProvider)
        services.AddSingleton<IAccessTokenProvider, DefaultAccessTokenProvider>();
        services.AddSingleton<TokenRefreshHandler>();
        services.AddSingleton<TokenRefreshLock>();

        // Read default client policy settings from configuration with sensible fallbacks
        var defaultSection = configuration.GetSection("HttpClients:DefaultClient");
        var defaultRetryCount = defaultSection.GetValue<int?>("RetryCount") ?? 3;
        var defaultTimeoutSeconds = defaultSection.GetValue<int?>("TimeoutSeconds") ?? 30;
        var defaultCircuitFailures = defaultSection.GetValue<int?>("CircuitFailures") ?? 5;
        var defaultCircuitDuration = TimeSpan.FromSeconds(defaultSection.GetValue<int?>("CircuitDurationSeconds") ?? 30);

        // Named default client
        services.AddHttpClient("DefaultClient", client =>
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(100); // base HttpClient timeout
        })
        .AddHttpMessageHandler<TokenRefreshHandler>()
        .AddPolicyHandler(GetRetryPolicy(defaultRetryCount))
        .AddPolicyHandler(GetRetryOnUnauthorizedPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy(defaultCircuitFailures, defaultCircuitDuration))
        .AddPolicyHandler(GetTimeoutPolicy(defaultTimeoutSeconds));

        // Example typed client for an external API. Configure base address and per-client policies
        var externalSection = configuration.GetSection("HttpClients:ExternalApi");
        var externalBase = externalSection.GetValue<string>("BaseAddress");
        var externalRetry = externalSection.GetValue<int?>("RetryCount") ?? 3;
        var externalTimeout = externalSection.GetValue<int?>("TimeoutSeconds") ?? 30;
        var externalCircuitFailures = externalSection.GetValue<int?>("CircuitFailures") ?? 5;
        var externalCircuitDuration = TimeSpan.FromSeconds(externalSection.GetValue<int?>("CircuitDurationSeconds") ?? 30);

        services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
        {
            if (!string.IsNullOrEmpty(externalBase))
                client.BaseAddress = new Uri(externalBase);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<TokenRefreshHandler>()
        .AddPolicyHandler(GetRetryPolicy(externalRetry))
        .AddPolicyHandler(GetRetryOnUnauthorizedPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy(externalCircuitFailures, externalCircuitDuration))
        .AddPolicyHandler(GetTimeoutPolicy(externalTimeout));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        // Retry for HTTP 5xx and network failures with exponential backoff
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryOnUnauthorizedPolicy()
    {
        // Retry once on 401 Unauthorized. We generally let TokenRefreshHandler coordinate refresh,
        // so having this policy is optional. Keep it as a short retry to surface refreshed tokens.
        return Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                     .RetryAsync(1);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        // Break circuit after a number of consecutive transient errors
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, durationOfBreak);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int seconds)
    {
        // Limit time for a single call. This is in addition to HttpClient.Timeout
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(seconds), TimeoutStrategy.Pessimistic);
    }
}
