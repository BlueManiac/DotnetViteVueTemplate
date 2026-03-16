using Microsoft.AspNetCore.Authentication.BearerToken;
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

        group.MapPost("/login", static (LoginRequest request, ILogger<PasswordAuthModule> logger) =>
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Password authentication login: {User}", request.UserName);
            }

            var user = UserPrincipal.Create();

            user.Email = request.UserName;
            user.Name = request.UserName;
            user.Provider = PROVIDER_NAME;

            return TypedResults.SignIn(user.Principal);
        })
        .AllowAnonymous();
    }
}
