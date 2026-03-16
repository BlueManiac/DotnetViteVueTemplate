using System.Security.Claims;

namespace Web.Features.Auth.Microsoft;

/// <summary>
/// Provides access to Microsoft OAuth tokens from the current user's claims.
/// </summary>
public class MicrosoftTokenProvider(IHttpContextAccessor httpContextAccessor)
{
    public const string CLAIM_ACCESS_TOKEN = "microsoft_access_token";
    public const string CLAIM_REFRESH_TOKEN = "microsoft_refresh_token";
    public const string CLAIM_OBJECT_ID = "microsoft_object_id";

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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
}
