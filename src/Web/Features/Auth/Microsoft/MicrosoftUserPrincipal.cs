using System.Security.Claims;

namespace Web.Features.Auth.Microsoft;

public class MicrosoftUserPrincipal : UserPrincipal
{
    public const string CLAIM_OBJECT_ID = "oid";
    public const string CLAIM_ACCESS_TOKEN = "microsoft_access_token";
    public const string CLAIM_REFRESH_TOKEN = "microsoft_refresh_token";
    public const string CLAIM_ACCESS_TOKEN_EXPIRES_AT = "microsoft_access_token_expires_at";

    private readonly ILogger<MicrosoftUserPrincipal> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MicrosoftUserPrincipal(
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider) : base(principal)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<MicrosoftUserPrincipal>>();
    }

    /// <summary>
    /// Gets or sets the user's email address.
    /// Checks Microsoft's common claim types: ClaimTypes.Email and "email".
    /// </summary>
    public new string? Email
    {
        get => _principal.FindFirstValue(ClaimTypes.Email)
            ?? _principal.FindFirstValue("email");
        set => SetClaim(ClaimTypes.Email, value);
    }

    /// <summary>
    /// Gets or sets the user's display name.
    /// Checks Microsoft's common claim types: ClaimTypes.Name and "name".
    /// </summary>
    public new string? Name
    {
        get => _principal.FindFirstValue(ClaimTypes.Name)
            ?? _principal.FindFirstValue("name");
        set => SetClaim(ClaimTypes.Name, value);
    }

    /// <summary>
    /// Gets or sets the Microsoft Azure AD object ID (oid) for the user.
    /// Checks Microsoft's claim types: "oid" and "http://schemas.microsoft.com/identity/claims/objectidentifier".
    /// This can be used to filter call records and other user-specific data.
    /// </summary>
    public string? ObjectId
    {
        get => _principal.FindFirstValue("oid")
            ?? _principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        set => SetClaim("oid", value);
    }

    /// <summary>
    /// Gets or sets the Microsoft access token for the user.
    /// This token can be used to call Microsoft Graph API on behalf of the user.
    /// </summary>
    public string? AccessToken
    {
        get => _principal.FindFirstValue(CLAIM_ACCESS_TOKEN);
        set
        {
            SetClaim(CLAIM_ACCESS_TOKEN, value);
            if (!string.IsNullOrEmpty(value))
            {
                _logger.LogDebug("Microsoft tokens stored");
            }
        }
    }

    /// <summary>
    /// Gets or sets the Microsoft refresh token for the user.
    /// </summary>
    public string? RefreshToken
    {
        get => _principal.FindFirstValue(CLAIM_REFRESH_TOKEN);
        set => SetClaim(CLAIM_REFRESH_TOKEN, value);
    }

    /// <summary>
    /// Gets or sets the expiration date/time of the Microsoft access token.
    /// </summary>
    public DateTimeOffset? AccessTokenExpiresAt
    {
        get
        {
            var expiresAtString = _principal.FindFirstValue(CLAIM_ACCESS_TOKEN_EXPIRES_AT);
            if (expiresAtString != null && DateTimeOffset.TryParse(expiresAtString, out var expiresAt))
            {
                return expiresAt;
            }
            return null;
        }
        set
        {
            SetClaim(CLAIM_ACCESS_TOKEN_EXPIRES_AT, value?.ToString("o"));

            if (!value.HasValue)
            {
                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var expiresInMinutes = (value.Value - DateTimeOffset.UtcNow).TotalMinutes;
                _logger.LogDebug("Microsoft Access Token expires in {Minutes} minutes", (int)expiresInMinutes);
            }

            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            var jwtExpirationMinutes = configuration.GetValue("Authentication:AccessTokenExpirationMinutes", 60);
            var jwtExpiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtExpirationMinutes);

            if (value.Value < jwtExpiresAt)
            {
                var timeDifference = (jwtExpiresAt - value.Value).TotalMinutes;
                _logger.LogWarning(
                    "Microsoft access token expires {Minutes} minutes before JWT token. " +
                    "The Microsoft token will be automatically refreshed during JWT token refresh.",
                    (int)timeDifference
                );
            }
        }
    }
}
