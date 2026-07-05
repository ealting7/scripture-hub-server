using System.ComponentModel.DataAnnotations;

namespace scripture_hub_server.Infrastructure.Data.Models.Auth;

public sealed class AppUser
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(128)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
