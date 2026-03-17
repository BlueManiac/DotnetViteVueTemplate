using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Tokens.Commands;

public record UserTokenRotateRequest(string RefreshToken);

public record UserTokenRotateResponse(Guid UserId, string? DeviceInfo);

public class UserTokenRotateCommand(DbContext db) : ICommand<UserTokenRotateRequest, UserTokenRotateResponse>
{
    public async Task<UserTokenRotateResponse> ExecuteAsync(
        UserTokenRotateRequest request,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var token = await db.Set<UserToken>()
            .Where(t => t.Token == request.RefreshToken && t.RevokedAt == null && t.ExpiresAt > now)
            .Select(t => new { t.UserId, t.DeviceInfo })
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");

        await db.Set<UserToken>()
            .Where(t => t.Token == request.RefreshToken)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

        return new UserTokenRotateResponse(token.UserId, token.DeviceInfo);
    }
}
