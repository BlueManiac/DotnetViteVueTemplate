using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth;

public class AuthModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(options =>
            {
                // Set Bearer Token as the default scheme for API authentication
                options.DefaultScheme = BearerTokenDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = BearerTokenDefaults.AuthenticationScheme;
            })
            .AddBearerToken(BearerTokenDefaults.AuthenticationScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
                options.RefreshTokenExpiration = TimeSpan.FromMinutes(60);
            });

        builder.Services.AddAuthorization(options =>
        {
            // Require authentication by default for all endpoints
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public record LoginRequest(string UserName, string Password);
    public record UserResponse(string Name);

    public static void MapRoutes(WebApplication app)
    {
        app.UseAuthorization();

        var group = app.MapGroup("auth");

        group.MapPost("/login", (LoginRequest request) =>
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, request.UserName),
                new(ClaimTypes.Name, request.UserName)
            };

            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, BearerTokenDefaults.AuthenticationScheme)
            );

            return TypedResults.SignIn(claimsPrincipal);
        })
        .AllowAnonymous();

        group.MapPost("/refresh", (RefreshRequest refreshRequest, IOptionsMonitor<BearerTokenOptions> optionsMonitor, TimeProvider timeProvider) =>
        {
            var identityBearerOptions = optionsMonitor.Get(BearerTokenDefaults.AuthenticationScheme);
            var refreshTicket = identityBearerOptions.RefreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc || timeProvider.GetUtcNow() >= expiresUtc)
            {
                return Results.Challenge();
            }

            return TypedResults.SignIn(refreshTicket.Principal);
        })
        .AllowAnonymous();

        group.MapGet("/user", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.Name is null)
            {
                return Results.Unauthorized();
            }

            return TypedResults.Ok(new UserResponse(user.Identity.Name));
        });
    }
}
