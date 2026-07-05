namespace scripture_hub_server.Infrastructure.Data.Models.Auth
{
    public sealed record LoginRequest(string Email, string Password);

    public sealed record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName
    );

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        UserDto? User,
        bool Success,
        string? ErrorMessage
    );

    public sealed record UserDto(
        string Id,
        string Email,
        string FirstName,
        string LastName
    );
}
