using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Web.Features.Auth.Microsoft;

/// <summary>
/// Provides access to Microsoft OAuth tokens from the current user's claims.
/// </summary>
public class MicrosoftTokenProvider(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider,
    ILogger<MicrosoftTokenProvider> logger)
{
    public const string CLAIM_ACCESS_TOKEN = "microsoft_access_token";
    public const string CLAIM_REFRESH_TOKEN = "microsoft_refresh_token";
    public const string CLAIM_OBJECT_ID = "microsoft_object_id";
    public const string CLAIM_ACCESS_TOKEN_EXPIRES_AT = "microsoft_access_token_expires_at";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<MicrosoftTokenProvider> _logger = logger;

    /// <summary>
    /// Gets the Microsoft access token for the currently authenticated user.
    /// This token can be used to call Microsoft Graph API on behalf of the user.
    /// </summary>
    public string? AccessToken
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == CLAIM_ACCESS_TOKEN)?.Value;
        }
    }

    /// <summary>
    /// Gets the Microsoft refresh token for the currently authenticated user.
    /// </summary>
    public string? RefreshToken
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == CLAIM_REFRESH_TOKEN)?.Value;
        }
    }

    /// <summary>
    /// Gets the Microsoft Azure AD object ID (oid) for the currently authenticated user.
    /// This can be used to filter call records and other user-specific data.
    /// </summary>
    public string? ObjectId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == CLAIM_OBJECT_ID)?.Value;
        }
    }

    /// <summary>
    /// Gets the expiration date/time of the Microsoft access token.
    /// Returns null if no expiration date is stored.
    /// </summary>
    public DateTimeOffset? AccessTokenExpiresAt
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var expiresAtString = user?.Claims.FirstOrDefault(c => c.Type == CLAIM_ACCESS_TOKEN_EXPIRES_AT)?.Value;
            if (expiresAtString != null && DateTimeOffset.TryParse(expiresAtString, out var expiresAt))
            {
                return expiresAt;
            }
            return null;
        }
    }

    /// <summary>
    /// Stores Microsoft OAuth tokens and expiration as claims.
    /// Handles both initial login and token refresh scenarios.
    /// </summary>
    /// <param name="identity">The claims identity to update</param>
    /// <param name="accessToken">The new access token</param>
    /// <param name="expiresInSeconds">Number of seconds until the access token expires</param>
    /// <param name="refreshToken">Optional new refresh token</param>
    public void StoreTokens(ClaimsIdentity identity, string accessToken, int expiresInSeconds, string? refreshToken = null)
    {
        // Update access token
        identity.TryRemoveClaim(identity.FindFirst(CLAIM_ACCESS_TOKEN));
        identity.AddClaim(new Claim(CLAIM_ACCESS_TOKEN, accessToken));

        // Update expiration
        StoreTokenExpiration(identity, expiresInSeconds);

        // Update refresh token if provided
        if (!string.IsNullOrEmpty(refreshToken))
        {
            identity.TryRemoveClaim(identity.FindFirst(CLAIM_REFRESH_TOKEN));
            identity.AddClaim(new Claim(CLAIM_REFRESH_TOKEN, refreshToken));
        }

        _logger.LogDebug("Microsoft tokens stored");
    }

    /// <summary>
    /// Stores the Microsoft access token expiration time as a claim and checks if it expires before the bearer token.
    /// </summary>
    /// <param name="identity">The claims identity to add the expiration claim to</param>
    /// <param name="expiresInSeconds">Number of seconds until the Microsoft token expires</param>
    public void StoreTokenExpiration(ClaimsIdentity identity, int expiresInSeconds)
    {
        identity.TryRemoveClaim(identity.FindFirst(CLAIM_ACCESS_TOKEN_EXPIRES_AT));

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
        var expiryClaim = new Claim(CLAIM_ACCESS_TOKEN_EXPIRES_AT, expiresAt.ToString("o"));
        identity.AddClaim(expiryClaim);

        _logger.LogDebug("Microsoft Access Token expires in {Minutes} minutes", expiresInSeconds / 60);

        var bearerTokenOptions = _serviceProvider
            .GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()
            .Get(BearerTokenDefaults.AuthenticationScheme);

        var bearerTokenExpiresAt = DateTimeOffset.UtcNow.Add(bearerTokenOptions.BearerTokenExpiration);

        if (expiresAt < bearerTokenExpiresAt)
        {
            var timeDifference = (bearerTokenExpiresAt - expiresAt).TotalMinutes;
            _logger.LogWarning(
                "Microsoft access token expires {Minutes} minutes before bearer token. " +
                "The Microsoft token will be automatically refreshed during bearer token refresh.",
                (int)timeDifference
            );
        }
    }
}
