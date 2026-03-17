using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Tokens.Commands;

public record UserTokenCleanupRequest();

public class UserTokenCleanupCommand(DbContext db) : ICommand<UserTokenCleanupRequest, int>
{
    public async Task<int> ExecuteAsync(UserTokenCleanupRequest request, CancellationToken ct)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30); // Keep revoked tokens for 30 days for audit

        return await db.Set<UserToken>()
            .Where(t =>
                (t.RevokedAt == null && t.ExpiresAt < DateTime.UtcNow) ||
                (t.RevokedAt != null && t.RevokedAt < cutoffDate))
            .ExecuteDeleteAsync(ct);
    }
}
