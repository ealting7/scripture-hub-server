using System.IdentityModel.Tokens.Jwt;

namespace scripture_hub_server.Infrastructure.Middleware;

public class AuthDiagnosticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthDiagnosticsMiddleware> _logger;

    public AuthDiagnosticsMiddleware(RequestDelegate next, ILogger<AuthDiagnosticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        _logger.LogInformation("=== AUTH DIAGNOSTICS ===");
        _logger.LogInformation($"Authorization Header Present: {!string.IsNullOrEmpty(authHeader)}");

        if (!string.IsNullOrEmpty(authHeader))
        {
            _logger.LogInformation($"Header Value: {authHeader}");

            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation($"Token extracted: {token.Substring(0, Math.Min(50, token.Length))}...");

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                    _logger.LogInformation($"Token Algorithm: {jwtToken?.Header.Alg}");
                    _logger.LogInformation($"Token Issuer: {jwtToken?.Issuer}");
                    _logger.LogInformation($"Token Audience: {string.Join(", ", jwtToken?.Audiences ?? new List<string>())}");
                    _logger.LogInformation($"Token Expiration: {jwtToken?.ValidTo:O}");
                    _logger.LogInformation($"Token Claims: {string.Join(", ", jwtToken?.Claims.Select(c => $"{c.Type}={c.Value}") ?? new List<string>())}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Token Read Error: {ex.Message}");
                }
            }
        }

        _logger.LogInformation($"User.Identity.IsAuthenticated: {context.User?.Identity?.IsAuthenticated}");
        _logger.LogInformation($"User.Identity.AuthenticationType: {context.User?.Identity?.AuthenticationType}");
        _logger.LogInformation($"User Principal Claims: {string.Join(", ", context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? new List<string>())}");
        _logger.LogInformation("======================");

        await _next(context);
    }
}
