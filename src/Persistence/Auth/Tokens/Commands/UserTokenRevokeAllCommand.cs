using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Tokens.Commands;

public record UserTokenRevokeAllRequest(Guid UserId);

public class UserTokenRevokeAllCommand(DbContext db) : ICommand<UserTokenRevokeAllRequest, int>
{
    public async Task<int> ExecuteAsync(UserTokenRevokeAllRequest request, CancellationToken ct)
    {
        return await db.Set<UserToken>()
            .Where(t => t.UserId == request.UserId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);
    }
}
