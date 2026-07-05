using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using scripture_hub_server.Infrastructure.Data.Context;
using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Infrastructure.Middleware;

public static class IdentityDependency
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DB Contexts
        services.AddDbContext<UserIdentityDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Identity (using AddIdentityCore instead of AddIdentity to avoid Cookies authentication scheme)
        services.AddIdentityCore<UserIdentity>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UserIdentityDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
