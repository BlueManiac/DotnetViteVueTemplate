using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Persistence.Auth;
using Persistence.Auth.Claims;
using Persistence.Auth.Tokens.Commands;
using Persistence.Auth.Users;
using Persistence.Auth.Users.Queries;
using Persistence.Shared.Cqrs;
using System.Security.Claims;
using System.Security.Cryptography;
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
                    },
                    OnTokenValidated = async context =>
                    {
                        var services = context.HttpContext.RequestServices;
                        var user = new UserPrincipal(context.Principal!);
                        var loader = services.GetRequiredService<UserClaimService>();

                        var loadedClaims = await loader.GetClaimsAsync(user);
                        if (loadedClaims.Count == 0) return;

                        var identity = user.Identity;
                        if (identity == null) return;

                        var loadedTypes = new HashSet<string>(loadedClaims.Select(c => c.Type));
                        var toRemove = identity.Claims.Where(c => loadedTypes.Contains(c.Type)).ToList();
                        foreach (var claim in toRemove) identity.TryRemoveClaim(claim);

                        identity.AddClaims(loadedClaims);
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

        // Needed for auth token exchange caching
        builder.Services.AddMemoryCache();
    }

    public record UserResponse(string Name, string? Email, string? Provider);
    public record ExchangeCodeRequest(string Code);

    private static string ExchangeCacheKey(string code)
    {
        return $"auth_exchange:{code}";
    }

    /// <summary>
    /// Issues tokens, stores them under a short-lived one-time code, and redirects to the frontend
    /// with only the code in the URL. The frontend exchanges the code for tokens via POST /auth/exchange.
    /// </summary>
    public static async Task<IResult> CreateTokenRedirect(
        UserPrincipal user,
        HttpContext context,
        IConfiguration configuration,
        UserTokenService tokenService,
        AuthenticationProperties? authProperties,
        IMemoryCache cache)
    {
        var tokens = await tokenService.IssueTokensAsync(user, context.Request.Headers.UserAgent.ToString());

        var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        cache.Set(ExchangeCacheKey(code), tokens, TimeSpan.FromMinutes(5));

        var queryParams = new Dictionary<string, string?> { ["code"] = code };

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
            UserClaimService claimsLoader,
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

                // Load all DB-backed claims (e.g. provider tokens) into the fresh principal
                user.Identity?.AddClaims(await claimsLoader.GetClaimsAsync(user));

                // Refresh external provider tokens if authenticated via external provider
                var refresher = tokenRefreshers.FirstOrDefault(r => r.ProviderName == user.Provider);
                if (refresher != null)
                {
                    try
                    {
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

        group.MapPost("/exchange", (ExchangeCodeRequest request, IMemoryCache cache) =>
        {
            var cacheKey = ExchangeCacheKey(request.Code);
            if (!cache.TryGetValue<UserTokenService.TokenResponse>(cacheKey, out var tokens) || tokens is null)
            {
                return Results.Unauthorized();
            }

            cache.Remove(cacheKey);
            return TypedResults.Ok(tokens);
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
