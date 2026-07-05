using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Infrastructure.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IUserContextAccessorService accessor)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userContext = new UserContext
                {
                    UserId = context.User.FindFirst("oid")?.Value ?? string.Empty,
                    Email = context.User.FindFirst("preferred_username")?.Value ?? string.Empty,
                    DisplayName = context.User.FindFirst("name")?.Value ?? string.Empty,
                    Roles = context.User.FindAll("roles").Select(r => r.Value)
                };

                accessor.SetUserContext(userContext);
            }

            await _next(context);
        }
    }
}
