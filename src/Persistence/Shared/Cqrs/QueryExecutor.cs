using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Shared.Cqrs;

public class QueryExecutor(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResult> Execute<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
    {
        var query = _serviceProvider.GetService<IQuery<TRequest, TResult>>();
        if (query is not null)
        {
            return query.Execute(request, cancellationToken);
        }

        var asyncQuery = _serviceProvider.GetService<IQuery<TRequest, Task<TResult>>>();
        if (asyncQuery is not null)
        {
            return await asyncQuery.Execute(request, cancellationToken);
        }

        var queryableQuery = _serviceProvider.GetService<IQuery<TRequest, IQueryable<TResult>>>();
        if (queryableQuery is not null)
        {
            var queryable = queryableQuery.Execute(request, cancellationToken);
            return (await queryable.FirstOrDefaultAsync(cancellationToken))!;
        }

        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
        {
            var innerType = typeof(TResult).GetGenericArguments()[0];
            var queryableType = typeof(IQueryable<>).MakeGenericType(innerType);
            var queryType = typeof(IQuery<,>).MakeGenericType(typeof(TRequest), queryableType);

            var queryHandler = _serviceProvider.GetService(queryType);
            if (queryHandler is not null)
            {
                dynamic handler = queryHandler;
                var queryable = handler.Execute(request, cancellationToken);
                return (TResult)EntityFrameworkQueryableExtensions.AsAsyncEnumerable(queryable);
            }
        }

        throw new InvalidOperationException($"No query handler found for request '{typeof(TRequest).Name}' with result '{typeof(TResult).Name}', 'Task<{typeof(TResult).Name}>', or 'IQueryable<{typeof(TResult).Name}>'");
    }
}
