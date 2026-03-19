using Persistence.Auth.Claims.Commands;
using Persistence.Shared.Cqrs;
using System.Collections.Immutable;
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
    /// Claim types that should be included in the JWT in addition to the base <see cref="UserPrincipal.BaseClaimTypes"/>.
    /// All other provider-specific claims are excluded by default.
    /// </summary>
    IReadOnlySet<string> PublicClaimTypes => ImmutableHashSet<string>.Empty;

    /// <summary>
    /// Claim types that are persisted to the database and excluded from the JWT.
    /// The default <see cref="PersistTokensAsync"/> implementation automatically saves these claims.
    /// They are reloaded into the principal by <see cref="UserClaimService"/> on the next request.
    /// </summary>
    IReadOnlySet<string> PrivateClaimTypes => ImmutableHashSet<string>.Empty;

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
    async Task PersistTokensAsync(UserPrincipal user, CommandExecutor commandExecutor)
    {
        if (PrivateClaimTypes.Count == 0 || user.UserId == null) return;

        var claims = user.Principal.Claims
            .Where(c => PrivateClaimTypes.Contains(c.Type))
            .Select(c => (c.Type, (string?)c.Value))
            .ToArray();

        if (claims.Length == 0) return;

        await commandExecutor.Execute(
            new UserClaimsSetRequest(user.UserId.Value, ProviderName, claims)
        );
    }

}
