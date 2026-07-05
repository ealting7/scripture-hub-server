using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Application.Options;
using scripture_hub_server.Infrastructure.Data.Context;
using scripture_hub_server.Infrastructure.Data.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace scripture_hub_server.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly ScriptureHubDbContext _dbContext;
    private readonly UserIdentityDbContext _identityDbContext;
    private readonly JwtOptions _options;
    private readonly ILogger<BibleService> _logger;

    public JwtTokenService(ScriptureHubDbContext dbContext, UserIdentityDbContext identityDbContext, IOptions<JwtOptions> options, ILogger<BibleService> logger)
    {
        _dbContext = dbContext;
        _identityDbContext = identityDbContext;
        _options = options.Value;
        _logger = logger;
    }

    //public async Task<AuthResponse> GenerateTokensAsync(AppUser user, string ipAddress, CancellationToken cancellationToken = default)
    public async Task<AuthResponse> GenerateTokensAsync(UserIdentity user, string ipAddress, CancellationToken cancellationToken = default)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user, ipAddress, cancellationToken);

        var userDto = new UserDto(
            user.Id.ToString(),
            user.Email,
            user.FirstName,
            user.LastName
        );

        return new AuthResponse(accessToken, refreshToken.Token, userDto, true, null);
    }

    public async Task<AuthResponse> RefreshTokensAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _identityDbContext.RefreshTokens
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (tokenEntity is null ||
            tokenEntity.IsExpired ||
            tokenEntity.IsRevoked ||
            tokenEntity.IsBlacklisted)
        {
            throw new SecurityTokenException("Invalid refresh token.");
        }


        // rotation: revoke old, create new
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;

        var newRefreshToken = await CreateRefreshTokenAsync(tokenEntity.User, ipAddress, cancellationToken);
        tokenEntity.ReplacedByToken = newRefreshToken.Token;

        await _identityDbContext.SaveChangesAsync(cancellationToken);

        var accessToken = GenerateAccessToken(tokenEntity.User);

        var user = tokenEntity.User;
        var userDto = new UserDto(
            user.Id.ToString(),
            user.Email,
            user.FirstName,
            user.LastName
        );

        return new AuthResponse(accessToken, newRefreshToken.Token, userDto, true, null);
    }

    //private string GenerateAccessToken(AppUser user)
    private string GenerateAccessToken(UserIdentity user)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            // for UserContextMiddleware
            new("oid", user.Id.ToString()),
            new("preferred_username", user.Email),
            new("name", $"{user.FirstName} {user.LastName}".Trim()),
            // roles claim (empty for now)
            new("roles", "User")
        };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, @$"Failed to generate access token for user: {user.Email}");
            return string.Empty;
        }
    }

    //private async Task<RefreshToken> CreateRefreshTokenAsync(AppUser user, string ipAddress, CancellationToken cancellationToken)
    private async Task<RefreshToken> CreateRefreshTokenAsync(UserIdentity user, string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            };

            _identityDbContext.RefreshTokens.Add(refreshToken);
            await _identityDbContext.SaveChangesAsync(cancellationToken);

            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $@"Failed to create refresh token for user: {user.Email}");
            return new RefreshToken();
        }
    }
}
