using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Features.Auth;
using Web.Util.Modules;

namespace Web.Features.Auth.Microsoft;

/// <summary>
/// Microsoft Entra ID (Azure AD) authentication provider using OpenID Connect.
/// 
/// Setup Steps:
/// 1. Go to Azure Portal → Microsoft Entra ID → App registrations → New registration
/// 2. Set Redirect URI: https://localhost:7126/api/auth/microsoft-callback (or your domain)
/// 3. Note the Application (client) ID and Directory (tenant) ID
/// 4. Go to Certificates & secrets → New client secret → Copy the secret value
/// 5. Add configuration below to appsettings.Local.json
/// 6. Restart backend to pick up the new module
/// 
/// Configuration (appsettings.json):
/// {
///   "Authentication": {
///     "Microsoft": {
///       "ClientId": "your-app-client-id",
///       "ClientSecret": "your-client-secret",
///       "TenantId": "common",       // "common" - Work/school + personal accounts
///                                   // "consumers" - Personal Microsoft accounts only
///                                   // "organizations" - Work/school accounts only
///                                   // "<guid>" - Specific tenant ID for single organization
///       "Scopes": ["User.Read"]     // Optional additional scopes beyond openid, profile, email
///     }
///   }
/// }
/// </summary>
public class MicrosoftEntraAuthModule : IModule
{
    private static bool TryGetEntraAuthConfig(IConfiguration configuration, out string clientId, out string clientSecret, out string tenantId, out string[]? scopes)
    {
        clientId = configuration["Authentication:Microsoft:ClientId"] ?? string.Empty;
        clientSecret = configuration["Authentication:Microsoft:ClientSecret"] ?? string.Empty;
        tenantId = configuration["Authentication:Microsoft:TenantId"] ?? "common";
        scopes = configuration.GetSection("Authentication:Microsoft:Scopes").Get<string[]>();

        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret);
    }

    public static void AddServices(WebApplicationBuilder builder)
    {
        if (!TryGetEntraAuthConfig(builder.Configuration, out var clientId, out var clientSecret, out var tenantId, out var configuredScopes))
        {
            return;
        }

        // Extend existing authentication configuration
        var authBuilder = builder.Services.AddAuthentication();

        authBuilder
            .AddOpenIdConnect("Microsoft", options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                options.CallbackPath = "/api/auth/microsoft-callback";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = "code";

                // Request access to user profile and email
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Add any additional configured scopes
                if (configuredScopes is not null)
                {
                    foreach (var scope in configuredScopes)
                    {
                        options.Scope.Add(scope);
                    }
                }

                // Map claims from Microsoft identity
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "roles";

                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/auth/login?error=microsoft-auth-failed");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var configuration = routes.ServiceProvider.GetRequiredService<IConfiguration>();

        if (!TryGetEntraAuthConfig(configuration, out _, out _, out _, out _))
        {
            return;
        }

        // Register Microsoft as an available auth provider
        var providers = routes.ServiceProvider.GetRequiredService<AuthProviders>();
        providers.Register("microsoft");

        var group = routes.MapGroup("/auth");

        group.MapGet("/microsoft-login", (HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/microsoft-callback-handler"
            };

            var redirect = context.Request.Query["redirect"].ToString();
            if (!string.IsNullOrEmpty(redirect))
            {
                properties.Items["redirect"] = redirect;
            }

            return TypedResults.Challenge(
                properties: properties,
                authenticationSchemes: ["Microsoft"]
            );
        })
        .AllowAnonymous();

        group.MapGet("/microsoft-callback-handler", async (HttpContext context, IConfiguration configuration) =>
        {
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:7126";

            if (!authenticateResult.Succeeded)
            {
                return Results.Redirect($"{frontendUrl}/auth/login?error=microsoft-auth-failed");
            }

            var claims = authenticateResult.Principal.Claims;

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value!;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value!;

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

            var ticket = new AuthenticationTicket(claimsPrincipal, BearerTokenDefaults.AuthenticationScheme);

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
