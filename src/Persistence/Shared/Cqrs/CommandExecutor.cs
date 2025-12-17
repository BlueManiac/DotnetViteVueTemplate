using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Cqrs;

public class CommandExecutor(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResult> Execute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull
    {
        var command = _serviceProvider.GetRequiredService<ICommand<TRequest, TResult>>();
        return await command.ExecuteAsync(request, cancellationToken);
    }

    public async Task Execute<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : notnull
    {
        var command = _serviceProvider.GetRequiredService<ICommand<TRequest>>();
        await command.ExecuteAsync(request, cancellationToken);
    }
}