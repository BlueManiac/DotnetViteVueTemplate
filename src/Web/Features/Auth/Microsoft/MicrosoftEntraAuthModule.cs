using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

        // Register IHttpContextAccessor (required for accessing user claims)
        builder.Services.AddHttpContextAccessor();

        // Register HttpClient (required for token refresh)
        builder.Services.AddHttpClient();

        // Register Microsoft user principal for typed access to Microsoft claims
        builder.Services.AddScoped(static sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return new MicrosoftUserPrincipal(httpContext?.User ?? new ClaimsPrincipal(), sp);
        });

        // Register Microsoft token refresher for automatic token refresh
        builder.Services.AddScoped<IAuthProviderRefresher, MicrosoftTokenRefresher>();

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
                        var user = new MicrosoftUserPrincipal(context.Principal!, context.HttpContext.RequestServices);

                        using (logger.BeginScope(new Dictionary<string, object> { ["User"] = user.Email ?? "unknown" }))
                        {
                            var tokenResponse = context.TokenEndpointResponse;
                            if (tokenResponse?.AccessToken is null)
                            {
                                logger.LogWarning("Microsoft Access Token not received - check OAuth configuration");
                                return Task.CompletedTask;
                            }

                            if (string.IsNullOrEmpty(tokenResponse.ExpiresIn) || !int.TryParse(tokenResponse.ExpiresIn, out var expiresInSeconds))
                            {
                                logger.LogWarning("Microsoft Access Token expiration not received or invalid");
                                return Task.CompletedTask;
                            }

                            user.AccessToken = tokenResponse.AccessToken;
                            user.RefreshToken = tokenResponse.RefreshToken;
                            user.AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
                        }

                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<MicrosoftEntraAuthModule>>();
                        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                        var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:5043";

                        // Extract error details from the failure
                        var originalError = context.Failure?.Message ?? "Microsoft authentication failed";
                        var errorMessage = originalError;

                        // Extract AADSTS error code if present
                        var aadMatch = Regex.Match(originalError, @"AADSTS(\d+)");
                        var errorCode = aadMatch.Success
                            ? $"AADSTS{aadMatch.Groups[1].Value}"
                            : "microsoft-auth-failed";

                        // Provide user-friendly messages for common error codes
                        if (originalError.Contains("'access_denied'") || errorCode == "AADSTS65004")
                        {
                            errorMessage = $"You cancelled the sign-in process. Please try again if you want to sign in. ({errorCode})";
                        }
                        else if (errorCode == "AADSTS650053")
                        {
                            var scopeMatch = Regex.Match(originalError, @"asked for scope '([^']+)'");
                            if (scopeMatch.Success)
                            {
                                var scope = scopeMatch.Groups[1].Value;
                                errorMessage = $"The scope '{scope}' is not configured in the Azure AD application. Please contact your administrator to add this permission. ({errorCode})";
                            }
                            else
                            {
                                errorMessage = $"A required permission is not configured in the Azure AD application. Please contact your administrator. ({errorCode})";
                            }
                        }
                        else if (errorCode == "AADSTS50105")
                        {
                            errorMessage = $"Your account is not assigned to this application. Please contact your administrator for access. ({errorCode})";
                        }
                        else if (errorCode == "AADSTS50020")
                        {
                            errorMessage = $"The user account was not found in the directory. Please verify your email address or contact your administrator. ({errorCode})";
                        }
                        else if (errorCode == "AADSTS700016")
                        {
                            errorMessage = $"The application configuration is invalid. Please contact your administrator. ({errorCode})";
                        }
                        else if (errorCode != "microsoft-auth-failed" && errorMessage == originalError)
                        {
                            // For unhandled AADSTS errors, append the code for reference
                            errorMessage = $"{originalError} ({errorCode})";
                        }

                        logger.LogWarning("Microsoft authentication failed: {ErrorCode} - {Message}", errorCode, errorMessage);

                        var redirectUrl = QueryHelpers.AddQueryString(
                            $"{frontendUrl}/auth/login",
                            new Dictionary<string, string?>
                            {
                                ["errorMessage"] = errorMessage
                            }
                        );

                        context.Response.Redirect(redirectUrl);
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

        group.MapGet("/microsoft-callback-handler", async (
            HttpContext context,
            IConfiguration configuration,
            UserTokenService tokenService,
            IMemoryCache cache,
            ILogger<MicrosoftEntraAuthModule> logger) =>
        {
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:7126";

            if (!authenticateResult.Succeeded)
            {
                return Results.Redirect($"{frontendUrl}/auth/login?error=microsoft-auth-failed");
            }

            var cookieUser = new MicrosoftUserPrincipal(authenticateResult.Principal, context.RequestServices);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Microsoft account login: {User} (ObjectId: {ObjectId})", cookieUser.Email, cookieUser.ObjectId);
            }

            // Create clean bearer token principal with only necessary claims
            var cleanIdentity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            var cleanPrincipal = new ClaimsPrincipal(cleanIdentity);
            var user = new MicrosoftUserPrincipal(cleanPrincipal, context.RequestServices)
            {
                Email = cookieUser.Email,
                Name = cookieUser.Name,
                Provider = PROVIDER_NAME,
                ObjectId = cookieUser.ObjectId,
                AccessToken = cookieUser.AccessToken,
                RefreshToken = cookieUser.RefreshToken,
                AccessTokenExpiresAt = cookieUser.AccessTokenExpiresAt
            };

            return await AuthModule.CreateTokenRedirect(user, context, configuration, tokenService, authenticateResult.Properties, cache);
        })
        .AllowAnonymous();
    }
}
