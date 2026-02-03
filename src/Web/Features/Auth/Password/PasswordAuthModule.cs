using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth.Password;

public class PasswordAuthModule : IModule
{
    public const string PROVIDER_NAME = "password";

    public record LoginRequest(string UserName, string Password);

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        var configuration = routes.ServiceProvider.GetRequiredService<IConfiguration>();

        // Enable password authentication by default (true if not configured)
        if (!configuration.GetValue("Authentication:Password:Enabled", true))
        {
            return;
        }

        var providers = routes.ServiceProvider.GetRequiredService<AuthProviders>();
        providers.Register(PROVIDER_NAME);

        var group = routes.MapGroup("/auth");

        group.MapPost("/login", static (LoginRequest request) =>
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, request.UserName),
                new(ClaimTypes.Name, request.UserName),
                new(AuthModule.CLAIM_AUTH_PROVIDER, PROVIDER_NAME)
            };

            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, BearerTokenDefaults.AuthenticationScheme)
            );

            return TypedResults.SignIn(claimsPrincipal);
        })
        .AllowAnonymous();
    }
}
