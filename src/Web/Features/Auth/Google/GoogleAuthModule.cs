using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth.Google;

public class GoogleAuthModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        // Only configure Google authentication if credentials are provided
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            return;
        }

        // Extend existing authentication configuration
        var authBuilder = builder.Services.AddAuthentication();

        authBuilder
            // Cookie scheme to temporarily hold Google authentication info
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = "/auth/google-callback";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
    }

    public static void MapRoutes(WebApplication app)
    {
        var group = app.MapGroup("auth");

        group.MapGet("/google-login", () =>
        {
            return TypedResults.Challenge(
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/auth/google-callback-handler"
                },
                authenticationSchemes: [GoogleDefaults.AuthenticationScheme]
            );
        })
        .AllowAnonymous();

        group.MapGet("/google-callback-handler", async (HttpContext context, IConfiguration configuration) =>
        {
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:7126";

            if (!authenticateResult.Succeeded)
            {
                return Results.Redirect($"{frontendUrl}/auth/login?error=google-auth-failed");
            }

            var claims = authenticateResult.Principal.Claims;

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;

            var userClaims = new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, name)
            };

            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(userClaims, BearerTokenDefaults.AuthenticationScheme)
            );

            var bearerTokenOptions = context.RequestServices
                .GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()
                .Get(BearerTokenDefaults.AuthenticationScheme);

            var ticket = new AuthenticationTicket(
                claimsPrincipal,
                BearerTokenDefaults.AuthenticationScheme);

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

            var redirectUrl = QueryHelpers.AddQueryString($"{frontendUrl}/auth/login", queryParams);

            return Results.Redirect(redirectUrl);
        })
        .AllowAnonymous();
    }
}
