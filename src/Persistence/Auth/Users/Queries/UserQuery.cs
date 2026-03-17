using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Users.Queries;

public record UserRequest(
    Guid? UserId = null,
    IReadOnlyCollection<Guid>? UserIds = null,
    string? Email = null,
    string? Provider = null
);

public class UserQuery(DbContext db) : IQuery<UserRequest, IQueryable<User>>
{
    public IQueryable<User> Execute(UserRequest request, CancellationToken ct)
    {
        var query = db.Set<User>().AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(u => u.Id == request.UserId.Value);
        else if (request.UserIds != null)
            query = query.Where(u => request.UserIds.Contains(u.Id));

        if (request.Email != null)
            query = query.Where(u => u.Email == request.Email);
        if (request.Provider != null)
            query = query.Where(u => u.Provider == request.Provider);

        return query;
    }
}
