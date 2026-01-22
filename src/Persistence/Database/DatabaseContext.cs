using Microsoft.EntityFrameworkCore;

namespace Persistence.Database;

public class DatabaseContext(DbContextOptions<DatabaseContext> options, IEnumerable<IEntityConfiguration> configurations) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var configuration in configurations)
        {
            configuration.OnModelCreating(modelBuilder);
        }
    }
}
