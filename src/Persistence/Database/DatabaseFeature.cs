using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Persistence.Shared.Features;

namespace Persistence.Database;

public class DatabaseFeature<TContext> : IFeature
    where TContext : DbContext
{
    public static IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEntityConfigurations()
            .AddDataSeeders()
            .AddScoped<DbContext>(services => services.GetRequiredService<TContext>())
            .AddHostedService<DatabaseInitializationService>();

        var provider = configuration["Provider"] ?? "Sqlite";
        var connectionString = configuration["ConnectionString"] ?? "Data Source=:memory:";

        switch (provider)
        {
            case "Sqlite":
                if (connectionString.Contains(":memory:"))
                {
                    // Keep in-memory database alive with a singleton connection
                    services.TryAddSingleton(services =>
                    {
                        var connection = new SqliteConnection(connectionString);
                        connection.Open();
                        return connection;
                    });
                    services.AddDbContextPool<TContext>((services, options) => options.UseSqlite(services.GetRequiredService<SqliteConnection>()));
                }
                else
                {
                    services.AddDbContextPool<TContext>(options => options.UseSqlite(connectionString));
                }
                break;

            default:
                throw new InvalidOperationException($"Unsupported database provider: {provider}");
        }

        return services;
    }

    private class DatabaseInitializationService(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            var seeders = scope.ServiceProvider.GetServices<IDatabaseSeeder>();

            await db.Database.EnsureCreatedAsync(cancellationToken);

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(db, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
