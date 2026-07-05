namespace scripture_hub_server.Infrastructure.Middleware
{
    public static class CorsDependency
    {

        public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:AllowOrigins").Get<string[]>();

            if (origins == null || origins.Length == 0)
                throw new ArgumentNullException("Cors:AllowOrigins", "The 'Cors:AllowOrigins' configuration section is missing or empty.");

            services.AddCors(options =>
            {
                options.AddPolicy("ScriptureHubPolicy", policy =>
                {
                    policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
