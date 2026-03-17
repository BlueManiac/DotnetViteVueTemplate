using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Persistence.Auth;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth;

public class AuthModule : IModule
{
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
                options.RefreshTokenExpiration = TimeSpan.FromDays(7);

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<AuthProviders>();
        builder.Services.AddScoped(static sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return new UserPrincipal(httpContext?.User ?? new ClaimsPrincipal());
        });
        builder.Services.AddScoped<ICurrentUser>(static sp => sp.GetRequiredService<UserPrincipal>());
    }

    public record UserResponse(string Name, string? Email, string? Provider);

    /// <summary>
    /// Creates bearer tokens for a user principal and returns a redirect result to the frontend.
    /// </summary>
    public static IResult CreateBearerTokenRedirect(
        UserPrincipal user,
        HttpContext context,
        IConfiguration configuration,
        AuthenticationProperties? authProperties)
    {
        var bearerTokenOptions = context.RequestServices
            .GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()
            .Get(BearerTokenDefaults.AuthenticationScheme);

        var ticket = new AuthenticationTicket(user.Principal, BearerTokenDefaults.AuthenticationScheme);
        ticket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.Add(bearerTokenOptions.BearerTokenExpiration);

        var accessToken = bearerTokenOptions.BearerTokenProtector.Protect(ticket);
        var refreshToken = bearerTokenOptions.RefreshTokenProtector.Protect(ticket);

        var queryParams = new Dictionary<string, string?>
        {
            ["accessToken"] = accessToken,
            ["tokenType"] = "Bearer",
            ["expiresIn"] = ((long)bearerTokenOptions.BearerTokenExpiration.TotalSeconds).ToString(),
            ["refreshToken"] = refreshToken
        };

        if (authProperties?.Items.TryGetValue("redirect", out var redirect) == true)
        {
            queryParams["redirect"] = redirect;
        }

        var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:5043";
        var redirectUrl = QueryHelpers.AddQueryString($"{frontendUrl}/auth/login", queryParams);

        return Results.Redirect(redirectUrl);
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");

        group.MapPost("/refresh", async (
            RefreshRequest refreshRequest,
            IOptionsMonitor<BearerTokenOptions> optionsMonitor,
            TimeProvider timeProvider,
            IEnumerable<IAuthProviderRefresher> tokenRefreshers,
            ILogger<AuthModule> logger) =>
        {
            var identityBearerOptions = optionsMonitor.Get(BearerTokenDefaults.AuthenticationScheme);
            var refreshTicket = identityBearerOptions.RefreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc || timeProvider.GetUtcNow() >= expiresUtc)
            {
                return Results.Unauthorized();
            }

            var user = new UserPrincipal(refreshTicket.Principal);

            // Support refresh external provider tokens if authenticated via external provider
            var refresher = tokenRefreshers.FirstOrDefault(r => r.ProviderName == user.Provider);
            if (refresher != null)
            {
                try
                {
                    await refresher.RefreshTokensAsync(refreshTicket.Principal);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to refresh provider '{Provider}' tokens", user.Provider);
                }
            }

            return TypedResults.SignIn(refreshTicket.Principal);
        })
        .AllowAnonymous();

        group.MapGet("/providers", static (AuthProviders providers) =>
        {
            return TypedResults.Ok(new { providers = providers.Providers });
        })
        .AllowAnonymous();

        group.MapGet("/user", static (UserPrincipal user) =>
        {
            if (!user.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            return TypedResults.Ok(new UserResponse(user.Name ?? user.Email ?? "Unknown", user.Email, user.Provider));
        });

        group.MapPost("/logout", static async (HttpContext context) =>
        {
            await context.SignOutAsync(BearerTokenDefaults.AuthenticationScheme);
            return TypedResults.Ok();
        });
    }
}
