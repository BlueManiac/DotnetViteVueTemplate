using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Auth.Claims;
using Persistence.Auth.Tokens;
using Persistence.Auth.Users;
using Persistence.Database;
using Persistence.Shared.Features;

namespace Persistence.Auth;

public class AuthFeature : IFeature, IEntityConfiguration
{
    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddHostedService<UserTokenCleanupService>();
        services.AddScoped<UserClaimService>();

        return services;
    }

    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Provider });
        });
    }
}
