using Microsoft.AspNetCore.Identity;

namespace scripture_hub_server.Infrastructure.Data.Models.Auth;

public class UserIdentity : IdentityUser
{
    public override required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool EnabledNotifications { get; set; }

    public bool IsActive { get; set; } = true;
}