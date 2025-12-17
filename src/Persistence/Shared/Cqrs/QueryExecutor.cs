using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Cqrs;

public class QueryExecutor(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResult> Execute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
    {
        var query = _serviceProvider.GetRequiredService<IQuery<TRequest, TResult>>();
        return await query.ExecuteAsync(request, cancellationToken);
    }
}
