using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Application.Interfaces
{
    public interface IUserContextAccessorService
    {
        UserContext? UserContext { get; }
        void SetUserContext(UserContext context);
    }
}
