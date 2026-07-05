namespace scripture_hub_server.Infrastructure.Middleware
{
    public static class ScriptureHubRedisDependency
    {
        public static IServiceCollection AddScriptureHubRedisServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    configuration.GetConnectionString("Redis");

                options.InstanceName = "ScriptureHub:";
            });

            return services;
        }
    }
}
