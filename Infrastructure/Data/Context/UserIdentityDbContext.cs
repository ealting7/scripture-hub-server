using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using scripture_hub_server.Infrastructure.Data.Models.Auth;
using System.Reflection.Emit;

namespace scripture_hub_server.Infrastructure.Data.Context;

public class UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options) : IdentityDbContext<UserIdentity>(options)
{
    public DbSet<UserIdentity> ScriptureHubUsers => Set<UserIdentity>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Identity");

        builder.Entity<UserIdentity>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.EnabledNotifications).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(r => r.Token).IsUnique();
        });
    }
}
