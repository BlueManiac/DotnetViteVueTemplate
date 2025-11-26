using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Features.Auth;
using Web.Util.Modules;

namespace Web.Features.Auth.Google;

public class GoogleAuthModule : IModule
{
    private static bool TryGetGoogleAuthConfig(IConfiguration configuration, out string clientId, out string clientSecret)
    {
        clientId = configuration["Authentication:Google:ClientId"] ?? string.Empty;
        clientSecret = configuration["Authentication:Google:ClientSecret"] ?? string.Empty;

        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret);
    }

    public static void AddServices(WebApplicationBuilder builder)
    {
        if (!TryGetGoogleAuthConfig(builder.Configuration, out var clientId, out var clientSecret))
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
                options.CallbackPath = "/api/auth/google-callback";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var configuration = routes.ServiceProvider.GetRequiredService<IConfiguration>();

        if (!TryGetGoogleAuthConfig(configuration, out _, out _))
        {
            return;
        }

        // Register Google as an available auth provider
        var providers = routes.ServiceProvider.GetRequiredService<AuthProviders>();
        providers.Register("google");

        var group = routes.MapGroup("/auth");

        group.MapGet("/google-login", (HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google-callback-handler"
            };

            var redirect = context.Request.Query["redirect"].ToString();
            if (!string.IsNullOrEmpty(redirect))
            {
                properties.Items["redirect"] = redirect;
            }

            return TypedResults.Challenge(
                properties: properties,
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

            if (authenticateResult.Properties?.Items.TryGetValue("redirect", out var redirect) == true)
            {
                queryParams["redirect"] = redirect;
            }

            var redirectUrl = QueryHelpers.AddQueryString($"{frontendUrl}/auth/login", queryParams);

            return Results.Redirect(redirectUrl);
        })
        .AllowAnonymous();
    }
}
