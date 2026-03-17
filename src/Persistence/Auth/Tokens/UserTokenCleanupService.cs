using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Auth.Tokens.Commands;
using Persistence.Shared.Cqrs;

namespace Persistence.Auth.Tokens;

public class UserTokenCleanupService(IServiceProvider services, ILogger<UserTokenCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token cleanup service started");

        // Wait for the application to fully start before the first cleanup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var commandExecutor = scope.ServiceProvider.GetRequiredService<CommandExecutor>();

                var deletedCount = await commandExecutor.Execute<UserTokenCleanupRequest, int>(
                    new UserTokenCleanupRequest(),
                    stoppingToken
                );

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Cleaned up {Count} expired refresh tokens", deletedCount);
                }

                // Run cleanup daily
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error occurred during token cleanup");
            }
        }

        logger.LogInformation("Token cleanup service stopped");
    }
}
