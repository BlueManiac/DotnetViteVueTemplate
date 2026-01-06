using Microsoft.EntityFrameworkCore;

namespace Persistence.Database;

public interface IDatabaseSeeder
{
    Task SeedAsync(DbContext db, CancellationToken ct);
}
