using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Infrastructure.Data.Context;
using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ScriptureHubDbContext _dbContext;
        private readonly UserIdentityDbContext _identityDbContext;
        private readonly IJwtTokenService _jwtTokenService;
        //private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IPasswordHasher<UserIdentity> _passwordHasher;
        private readonly ILogger<AuthService> _logger;
        //public AuthService(UserIdentityDbContext identityDbContext, ScriptureHubDbContext dbContext,IJwtTokenService jwtTokenService, IPasswordHasher<AppUser> passwordHasher, ILogger<AuthService> logger)
        public AuthService(UserIdentityDbContext identityDbContext, ScriptureHubDbContext dbContext, IJwtTokenService jwtTokenService, IPasswordHasher<UserIdentity> passwordHasher, ILogger<AuthService> logger)
        {
            _identityDbContext = identityDbContext;
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var identityUser = await _identityDbContext.ScriptureHubUsers.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (identityUser is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            //var user = new AppUser 
            var user = new UserIdentity
            { 
                Id = identityUser.Id,
                Email = identityUser.UserName,
                PasswordHash = identityUser.PasswordHash,
                FirstName = identityUser.FirstName,
                LastName = identityUser.LastName,
                IsActive = true
            };

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("User is inactive.");

            return await _jwtTokenService.GenerateTokensAsync(user, ipAddress, cancellationToken);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                var existing = await _identityDbContext.ScriptureHubUsers.AnyAsync(u => u.Email == request.Email, cancellationToken);
                if (existing)
                    throw new InvalidOperationException("Email already in use.");

                var user = new UserIdentity
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                _identityDbContext.ScriptureHubUsers.Add(user);
                await _identityDbContext.SaveChangesAsync(cancellationToken);

                return await _jwtTokenService.GenerateTokensAsync(user, ipAddress, cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve translation book information:");
                return new AuthResponse ("", "", null, false, @$"{ex.Message}.");
            }
        }

        public Task<AuthResponse> RefreshAsync(RefreshRequest request, string ipAddress, CancellationToken cancellationToken = default)
        {
            return _jwtTokenService.RefreshTokensAsync(request.RefreshToken, ipAddress, cancellationToken);
        }

        public async Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
        {
            var tokenEntity = await _identityDbContext.RefreshTokens
                .SingleOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

            if (tokenEntity is null)
                return;

            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedByIp = ipAddress;
            tokenEntity.IsBlacklisted = true;

            await _identityDbContext.SaveChangesAsync(cancellationToken);

        }


    }
}