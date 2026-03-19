using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Claims.Commands;

public record UserClaimsSetRequest(
    Guid UserId,
    string Provider,
    IReadOnlyCollection<(string ClaimType, string? ClaimValue)> Claims
);

public class UserClaimsSetCommand(DbContext db) : ICommand<UserClaimsSetRequest>
{
    public async Task ExecuteAsync(UserClaimsSetRequest request, CancellationToken ct)
    {
        var claimTypes = request.Claims.Select(c => c.ClaimType).ToHashSet();

        var existing = await db.Set<UserClaim>()
            .Where(c => c.UserId == request.UserId && c.Provider == request.Provider && claimTypes.Contains(c.ClaimType))
            .ToDictionaryAsync(c => c.ClaimType, ct);

        foreach (var (claimType, claimValue) in request.Claims)
        {
            if (existing.TryGetValue(claimType, out var entity))
            {
                entity.ClaimValue = claimValue;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                db.Add(new UserClaim
                {
                    UserId = request.UserId,
                    Provider = request.Provider,
                    ClaimType = claimType,
                    ClaimValue = claimValue
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
