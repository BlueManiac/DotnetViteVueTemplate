using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Persistence.Auth;
using Persistence.Auth.Tokens.Commands;
using Persistence.Auth.Users;
using Persistence.Auth.Users.Queries;
using Persistence.Shared.Cqrs;
using System.Security.Claims;
using System.Text;
using Web.Util.Modules;

namespace Web.Features.Auth;

public class AuthModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key configuration is required");

        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters long");
        }

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Cookie scheme for temporary external authentication (Google, Microsoft, etc.)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Allow SignalR to pass tokens via query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/api"))
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
        builder.Services.AddScoped<UserTokenService>();
        builder.Services.AddScoped(static sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return new UserPrincipal(httpContext?.User ?? new ClaimsPrincipal());
        });
        builder.Services.AddScoped<ICurrentUser>(static sp => sp.GetRequiredService<UserPrincipal>());
    }

    public record UserResponse(string Name, string? Email, string? Provider);

    /// <summary>
    /// Creates JWT access token and database-backed refresh token, then redirects to the frontend.
    /// </summary>
    public static async Task<IResult> CreateTokenRedirect(
        UserPrincipal user,
        HttpContext context,
        IConfiguration configuration,
        UserTokenService tokenService,
        AuthenticationProperties? authProperties)
    {
        var tokens = await tokenService.IssueTokensAsync(user, context.Request.Headers.UserAgent.ToString());

        var queryParams = new Dictionary<string, string?>
        {
            ["accessToken"] = tokens.AccessToken,
            ["tokenType"] = tokens.TokenType,
            ["expiresIn"] = tokens.ExpiresIn.ToString(),
            ["refreshToken"] = tokens.RefreshToken
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
            UserTokenService tokenService,
            CommandExecutor commandExecutor,
            QueryExecutor queryExecutor,
            IEnumerable<IAuthProviderRefresher> tokenRefreshers,
            ILogger<AuthModule> logger) =>
        {
            try
            {
                var rotation = await commandExecutor.Execute<UserTokenRotateRequest, UserTokenRotateResponse>(
                    new UserTokenRotateRequest(refreshRequest.RefreshToken)
                );

                var userRecord = await queryExecutor.Execute<UserRequest, User?>(
                    new UserRequest(UserId: rotation.UserId)
                );

                if (userRecord == null)
                {
                    return Results.Unauthorized();
                }

                var user = UserPrincipal.Create();

                user.UserId = rotation.UserId;
                user.Email = userRecord.Email;
                user.Name = userRecord.Name;
                user.Provider = userRecord.Provider;

                // Support refresh external provider tokens if authenticated via external provider
                var refresher = tokenRefreshers.FirstOrDefault(r => r.ProviderName == user.Provider);
                if (refresher != null)
                {
                    try
                    {
                        // Provider tokens are stored in the DB, not in the JWT — load them before refreshing
                        await refresher.LoadTokensAsync(user, queryExecutor);
                        // Refresh the provider's access token (e.g. get a new Google/Microsoft token using the stored refresh token)
                        await refresher.RefreshTokensAsync(user.Principal);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to refresh provider '{Provider}' tokens", user.Provider);
                    }
                }

                // Rotate refresh token (create new, old one is already revoked)
                return TypedResults.Ok(await tokenService.IssueTokensAsync(user, rotation.DeviceInfo));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
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

        group.MapPost("/logout", async (
            CommandExecutor commandExecutor,
            ICurrentUser currentUser) =>
        {
            if (currentUser.UserId is { } userId)
            {
                await commandExecutor.Execute<UserTokenRevokeAllRequest, int>(
                    new UserTokenRevokeAllRequest(userId)
                );
            }

            return TypedResults.Ok();
        });
    }
}
