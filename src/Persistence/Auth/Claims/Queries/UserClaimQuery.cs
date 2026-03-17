using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Claims.Queries;

public record UserClaimRequest(Guid UserId, string Provider);

public class UserClaimQuery(DbContext db) : IQuery<UserClaimRequest, IQueryable<UserClaim>>
{
    public IQueryable<UserClaim> Execute(UserClaimRequest request, CancellationToken ct)
    {
        return db.Set<UserClaim>()
            .Where(c => c.UserId == request.UserId && c.Provider == request.Provider);
    }
}
