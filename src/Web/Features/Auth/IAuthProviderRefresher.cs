using System.Security.Claims;

namespace Web.Features.Auth;

/// <summary>
/// Interface for authentication providers to implement token refresh logic.
/// When the bearer token is refreshed via /api/auth/refresh, any registered provider
/// can refresh its own tokens (e.g., OAuth access tokens from Microsoft, Google, etc.).
/// </summary>
public interface IAuthProviderRefresher
{
    /// <summary>
    /// The name of the authentication provider (e.g., "microsoft", "google").
    /// Must match the value stored in the CLAIM_AUTH_PROVIDER claim.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Refreshes the provider's tokens and optionally updates related claims.
    /// The principal's claims can be modified in-place.
    /// </summary>
    /// <param name="principal">The claims principal containing the user's claims and tokens</param>
    /// <returns>True if the refresh was successful, false otherwise. Returning false will log a warning but not fail the bearer token refresh.</returns>
    Task<bool> RefreshTokensAsync(ClaimsPrincipal principal);
}
