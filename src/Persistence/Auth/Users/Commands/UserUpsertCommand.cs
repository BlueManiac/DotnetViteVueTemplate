using Microsoft.EntityFrameworkCore;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Users.Commands;

public record UserUpsertRequest(
    Guid? Id = null,
    string? Email = null,
    string? Name = null,
    string? Provider = null
);

public class UserUpsertCommand(DbContext db) : ICommand<UserUpsertRequest, Guid>
{
    public async Task<Guid> ExecuteAsync(UserUpsertRequest request, CancellationToken ct)
    {
        if (!request.Id.HasValue && request.Email == null)
        {
            throw new ArgumentException("Either Id or Email must be provided", nameof(request));
        }

        User? user = null;

        if (request.Id.HasValue)
        {
            user = await db.Set<User>().FindAsync([request.Id.Value], ct);
        }

        if (user == null && request.Email != null && request.Provider != null)
        {
            user = await db.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Provider == request.Provider, ct);
        }

        if (user == null)
        {
            user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Provider = request.Provider
            };
            db.Add(user);
        }
        else
        {
            user.Email = request.Email;
            user.Name = request.Name;
            user.Provider = request.Provider;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return user.Id;
    }
}
