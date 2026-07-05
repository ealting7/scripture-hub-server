using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Application.Services
{
    public class UserContextAccessorService : IUserContextAccessorService
    {
        public UserContext? UserContext { get; private set; }
        public void SetUserContext(UserContext context) => UserContext = context;
    }
}
