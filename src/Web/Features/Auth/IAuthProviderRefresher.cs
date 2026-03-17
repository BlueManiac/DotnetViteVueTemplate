using Persistence.Shared.Cqrs;
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
    /// Throw an exception to signal failure; the caller will log a warning.
    /// </summary>
    Task RefreshTokensAsync(ClaimsPrincipal principal);

    /// <summary>
    /// Persists provider-specific tokens to the database and strips them from the principal
    /// so they are not embedded in the JWT. Called during token issuance.
    /// </summary>
    Task PersistTokensAsync(UserPrincipal user, CommandExecutor commandExecutor) => Task.CompletedTask;

    /// <summary>
    /// Loads persisted provider tokens from the database into the principal's claims.
    /// Called before <see cref="RefreshTokensAsync"/> during JWT refresh.
    /// </summary>
    Task LoadTokensAsync(UserPrincipal user, QueryExecutor queryExecutor) => Task.CompletedTask;
}
