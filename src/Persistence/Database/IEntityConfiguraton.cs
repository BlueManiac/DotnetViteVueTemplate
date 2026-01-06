using Microsoft.EntityFrameworkCore;

namespace Persistence.Database;

public interface IEntityConfiguraton
{
    void OnModelCreating(ModelBuilder builder);
}
