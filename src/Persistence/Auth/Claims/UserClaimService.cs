using Microsoft.Extensions.Caching.Memory;
using Persistence.Auth.Claims.Queries;
using Persistence.Shared.Cqrs;
using System.Security.Claims;

namespace Persistence.Auth.Claims;

/// <summary>
/// Loads DB-backed claims for an authenticated user (e.g. provider tokens, roles).
/// Results are cached per user per provider for 5 minutes so the database is not hit on every request.
/// </summary>
public class UserClaimService(
    IMemoryCache cache,
    QueryExecutor queryExecutor)
{
    public async Task<IReadOnlyList<Claim>> GetClaimsAsync(ICurrentUser user)
    {
        var provider = user.Provider;
        var userId = user.UserId;

        if (string.IsNullOrEmpty(provider) || userId == null)
            return [];

        var cacheKey = $"user-claim:{provider}:{userId}";

        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            var claims = await queryExecutor.Execute<UserClaimRequest, List<UserClaim>>(
                new UserClaimRequest(userId.Value, provider)
            );

            return claims
                .Where(c => c.ClaimValue != null)
                .Select(c => new Claim(c.ClaimType, c.ClaimValue!))
                .ToList();
        });

        return result!;
    }

    public void Invalidate(string provider, Guid userId)
    {
        cache.Remove($"user-claim:{provider}:{userId}");
    }
}
