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
    public const string PROVIDER_NAME = "microsoft";

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

        // Register IHttpContextAccessor (required by MicrosoftTokenProvider)
        builder.Services.AddHttpContextAccessor();

        // Register Microsoft token provider for server-side use
        builder.Services.AddScoped<MicrosoftTokenProvider>();

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
                options.Scope.Add("offline_access"); // Required to receive refresh tokens

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
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<MicrosoftEntraAuthModule>>();
                        var email = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

                        using (logger.BeginScope(new Dictionary<string, object> { ["User"] = email ?? "unknown" }))
                        {
                            // Store Microsoft access token and refresh token as claims
                            // These can be retrieved later to call Microsoft Graph API
                            if (context.TokenEndpointResponse?.AccessToken is not null)
                            {
                                var claim = new Claim(MicrosoftTokenProvider.CLAIM_ACCESS_TOKEN, context.TokenEndpointResponse.AccessToken);
                                context.Principal?.Identities.FirstOrDefault()?.AddClaim(claim);
                                logger.LogDebug("Microsoft Access Token stored");
                            }
                            else
                            {
                                logger.LogWarning("Microsoft Access Token not received - check OAuth configuration");
                            }

                            if (context.TokenEndpointResponse?.RefreshToken is not null)
                            {
                                var claim = new Claim(MicrosoftTokenProvider.CLAIM_REFRESH_TOKEN, context.TokenEndpointResponse.RefreshToken);
                                context.Principal?.Identities.FirstOrDefault()?.AddClaim(claim);
                                logger.LogDebug("Microsoft Refresh Token stored");
                            }
                        }

                        return Task.CompletedTask;
                    },
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
        providers.Register(PROVIDER_NAME);

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

        group.MapGet("/microsoft-callback-handler", async (HttpContext context, IConfiguration configuration, ILogger<MicrosoftEntraAuthModule> logger) =>
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

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Microsoft account login: {User}", email);
            }

            var userClaims = new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, name),
                new(AuthModule.CLAIM_AUTH_PROVIDER, PROVIDER_NAME)
            };

            // Preserve Microsoft tokens in the bearer token for later use
            if (claims.FirstOrDefault(c => c.Type == MicrosoftTokenProvider.CLAIM_ACCESS_TOKEN)?.Value is string microsoftAccessToken)
            {
                userClaims.Add(new Claim(MicrosoftTokenProvider.CLAIM_ACCESS_TOKEN, microsoftAccessToken));
            }
            if (claims.FirstOrDefault(c => c.Type == MicrosoftTokenProvider.CLAIM_REFRESH_TOKEN)?.Value is string microsoftRefreshToken)
            {
                userClaims.Add(new Claim(MicrosoftTokenProvider.CLAIM_REFRESH_TOKEN, microsoftRefreshToken));
            }

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
