using Microsoft.EntityFrameworkCore;
using scripture_hub_server.Infrastructure.Data.Context;

namespace scripture_hub_server.Infrastructure.Middleware
{
    public static class ScriptureHubEFDependency
    {
        public static IServiceCollection AddScriptureHubEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ScriptureHubDbContext>(options =>            
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: new[]
                            {
                                40501, 10928, 10929, 40197, 40613, 49918, 49919, 49920,
                                10053, 10054, 10060, 64, 233,
                                1205, -2
                            }
                        );

                    }
            ));
            return services;
        }
    }
}
