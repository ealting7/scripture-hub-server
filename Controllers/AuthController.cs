using Microsoft.AspNetCore.Mvc;
using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Infrastructure.Data.Models.Auth;

namespace scripture_hub_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) =>
            _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request,CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, GetIpAddress(), cancellationToken);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request,CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, GetIpAddress(), cancellationToken);

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request,CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshAsync(request, GetIpAddress(), cancellationToken);

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(request.RefreshToken, GetIpAddress(), cancellationToken);
            return Ok(new { message = "Logged out" });
        }



        private string GetIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
                return forwarded.ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
