using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth;

public class AuthModule : IModule
{
    public const string CLAIM_AUTH_PROVIDER = "auth_provider";

    public static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(static options =>
            {
                // Set Bearer Token as the default scheme for API authentication
                options.DefaultScheme = BearerTokenDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = BearerTokenDefaults.AuthenticationScheme;
            })
            // Cookie scheme for temporary external authentication (Google, Microsoft, etc.)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddBearerToken(BearerTokenDefaults.AuthenticationScheme, static options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
                options.RefreshTokenExpiration = TimeSpan.FromMinutes(60);

                // Allow SignalR to pass tokens via query string
                options.Events = new()
                {
                    OnMessageReceived = static context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<AuthProviders>();
    }

    public record UserResponse(string Name, string? Provider);

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("/refresh", static (RefreshRequest refreshRequest, IOptionsMonitor<BearerTokenOptions> optionsMonitor, TimeProvider timeProvider) =>
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

        group.MapGet("/providers", static (AuthProviders providers) =>
        {
            return TypedResults.Ok(new { providers = providers.Providers });
        })
        .AllowAnonymous();

        group.MapGet("/user", static (ClaimsPrincipal user) =>
        {
            if (user.Identity?.Name is null)
            {
                return Results.Unauthorized();
            }

            var provider = user.Claims.FirstOrDefault(c => c.Type == CLAIM_AUTH_PROVIDER)?.Value;

            return TypedResults.Ok(new UserResponse(user.Identity.Name, provider));
        });
    }
}
