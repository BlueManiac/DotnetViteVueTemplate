using Microsoft.EntityFrameworkCore;

namespace Persistence.Database;

public interface IEntityConfiguration
{
    void OnModelCreating(ModelBuilder builder);
}
