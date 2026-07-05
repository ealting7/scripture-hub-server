using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace scripture_hub_server.Infrastructure.Middleware;

public static class AuthenticationDependency
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var useAzureAd = configuration.GetValue<bool>("UseAzureAd");

        if (useAzureAd)
        {
            services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));
        }
        else
        {
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key missing.");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
                        ValidAudience = jwtSection.GetValue<string>("Audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    // Log JWT Bearer validation events
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                            logger.LogInformation("✓ JWT Token validated successfully");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                            logger.LogError($"✗ JWT Authentication Failed: {context.Exception?.Message}");
                            logger.LogError($"  Exception Type: {context.Exception?.GetType().Name}");
                            if (context.Exception is SecurityTokenException ste)
                            {
                                logger.LogError($"  Token Exception: {ste.Message}");
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                            logger.LogWarning($"⚠ JWT Challenge: {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        return services;
    }
}
