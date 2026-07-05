using Microsoft.AspNetCore.Identity;
using scripture_hub_server.Application.Interfaces;
using scripture_hub_server.Application.Options;
using scripture_hub_server.Application.Services;
using scripture_hub_server.Infrastructure.Data.Models.Auth;
using scripture_hub_server.Infrastructure.Middleware;
using scripture_hub_server.Infrastructure.Http;

var builder = WebApplication.CreateBuilder(args);

//Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add JwtOptions
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
// Add JWT or Azure AD
builder.Services.AddAuthenticationServices(builder.Configuration);
// Add identity services
builder.Services.AddIdentityServices(builder.Configuration);
// Add Entity Framework services
builder.Services.AddScriptureHubEntityFrameworkServices(builder.Configuration);
// Add Redis services
builder.Services.AddScriptureHubRedisServices(builder.Configuration);
// Add CORS serves
builder.Services.AddCorsServices(builder.Configuration);
// Add HttpClient + Polly policies for resilience
builder.Services.AddHttpClientServices(builder.Configuration);

builder.Services.AddScoped<IUserContextAccessorService, UserContextAccessorService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IBibleService, BibleService>();    

// build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("ScriptureHubPolicy");

app.UseAuthentication();
app.UseAuthorization();

// For debug:
//app.UseMiddleware<AuthDiagnosticsMiddleware>();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();
app.Run();
