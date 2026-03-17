using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Shared.Cqrs;
using System.Security.Cryptography;

namespace Persistence.Auth.Tokens.Commands;

public record UserTokenCreateRequest(
    Guid UserId,
    string? DeviceInfo = null
);

public record UserTokenCreateResponse(string Token, DateTime ExpiresAt);

public class UserTokenCreateCommand(DbContext db, IConfiguration configuration) : ICommand<UserTokenCreateRequest, UserTokenCreateResponse>
{
    public async Task<UserTokenCreateResponse> ExecuteAsync(
        UserTokenCreateRequest request,
        CancellationToken ct)
    {
        var expirationDays = int.TryParse(configuration["Authentication:RefreshTokenExpirationDays"], out var days) ? days : 7;
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays);

        var refreshToken = new UserToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            UserId = request.UserId,
            ExpiresAt = expiresAt,
            DeviceInfo = request.DeviceInfo is { Length: > 500 } deviceInfo
                ? deviceInfo[..500]
                : request.DeviceInfo
        };

        db.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new UserTokenCreateResponse(refreshToken.Token, expiresAt);
    }
}
