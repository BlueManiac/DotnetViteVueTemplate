using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Web.Util.Modules;

namespace Web.Features.Auth.Google;

public class GoogleAuthModule : IModule
{
    public const string PROVIDER_NAME = "google";

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
        providers.Register(PROVIDER_NAME);

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

        group.MapGet("/google-callback-handler", async (
            HttpContext context,
            IConfiguration configuration,
            UserTokenService tokenService,
            ILogger<GoogleAuthModule> logger) =>
        {
            var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var frontendUrl = configuration["FrontendUrl"] ?? "https://localhost:7126";

            if (!authenticateResult.Succeeded)
            {
                return Results.Redirect($"{frontendUrl}/auth/login?error=google-auth-failed");
            }

            var cookieUser = new UserPrincipal(authenticateResult.Principal);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Google account login: {User}", cookieUser.Email);
            }

            var user = UserPrincipal.Create();

            user.Email = cookieUser.Email;
            user.Name = cookieUser.Name;
            user.Provider = PROVIDER_NAME;

            return await AuthModule.CreateTokenRedirect(user, context, configuration, tokenService, authenticateResult.Properties);
        })
        .AllowAnonymous();
    }
}
