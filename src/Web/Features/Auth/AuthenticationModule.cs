using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Web.Util.Modules;

namespace Web.Features.Auth;
public class AuthenticationModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddBearerToken(BearerTokenDefaults.AuthenticationScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(60);
            });
    }

    public record LoginRequest(string UserName, string Password);

    public static void MapRoutes(WebApplication app)
    {
        app.UseAuthorization();

        var group = app.MapGroup("auth");

        group.MapPost("/login", (LoginRequest request) =>
        {
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.Name, request.UserName)], BearerTokenDefaults.AuthenticationScheme)
            );

            return TypedResults.SignIn(claimsPrincipal);
        });

        group.MapPost("/refresh", (RefreshRequest refreshRequest, IOptionsMonitor<BearerTokenOptions> optionsMonitor, TimeProvider timeProvider) =>
        {
            var identityBearerOptions = optionsMonitor.Get(BearerTokenDefaults.AuthenticationScheme);
            var refreshTicket = identityBearerOptions.RefreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc || timeProvider.GetUtcNow() >= expiresUtc)
            {
                return Results.Challenge();
            }

            return TypedResults.SignIn(refreshTicket.Principal);
        });

        group.MapGet("/user", (ClaimsPrincipal user) =>
        {
            return user.Identity?.Name;
        })
        .RequireAuthorization();
    }
}
