using Persistence.Shared.Cqrs;

namespace Web.Util.Cqrs;

public static class CqrsRouteExtensions
{
    public static CommandBuilder<TRequest, TResult> Command<TRequest, TResult>(this IEndpointRouteBuilder group)
        where TRequest : notnull
        where TResult : class
    {
        return new CommandBuilder<TRequest, TResult>(group);
    }

    public static VoidCommandBuilder<TRequest> Command<TRequest>(this IEndpointRouteBuilder group)
        where TRequest : notnull
    {
        return new VoidCommandBuilder<TRequest>(group);
    }

    public static QueryBuilder<TRequest, TResult> Query<TRequest, TResult>(this IEndpointRouteBuilder group)
        where TRequest : notnull
        where TResult : class
    {
        return new QueryBuilder<TRequest, TResult>(group);
    }
}

public class CommandBuilder<TRequest, TResult>
    where TRequest : notnull
    where TResult : class
{
    private readonly IEndpointRouteBuilder _group;

    public CommandBuilder(IEndpointRouteBuilder group)
    {
        _group = group;
    }

    public RouteHandlerBuilder MapPost(string pattern)
    {
        return _group.MapPost(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request);
            return TypedResults.Ok(result);
        });
    }

    public RouteHandlerBuilder MapPut(string pattern)
    {
        return _group.MapPut(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request);
            return TypedResults.Ok(result);
        });
    }

    public RouteHandlerBuilder MapPatch(string pattern)
    {
        return _group.MapPatch(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request);
            return TypedResults.Ok(result);
        });
    }

    public RouteHandlerBuilder MapDelete(string pattern)
    {
        return _group.MapDelete(pattern, async ([AsParameters] TRequest request, CommandExecutor executor) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request);
            return TypedResults.Ok(result);
        });
    }
}

public class VoidCommandBuilder<TRequest>
    where TRequest : notnull
{
    private readonly IEndpointRouteBuilder _group;

    public VoidCommandBuilder(IEndpointRouteBuilder group)
    {
        _group = group;
    }

    public RouteHandlerBuilder MapPost(string pattern)
    {
        return _group.MapPost(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            await executor.Execute(request);
            return TypedResults.Ok();
        });
    }

    public RouteHandlerBuilder MapPut(string pattern)
    {
        return _group.MapPut(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            await executor.Execute(request);
            return TypedResults.Ok();
        });
    }

    public RouteHandlerBuilder MapPatch(string pattern)
    {
        return _group.MapPatch(pattern, async (TRequest request, CommandExecutor executor) =>
        {
            await executor.Execute(request);
            return TypedResults.Ok();
        });
    }

    public RouteHandlerBuilder MapDelete(string pattern)
    {
        return _group.MapDelete(pattern, async ([AsParameters] TRequest request, CommandExecutor executor) =>
        {
            await executor.Execute(request);
            return TypedResults.Ok();
        });
    }
}

public class QueryBuilder<TRequest, TResult>
    where TRequest : notnull
    where TResult : class
{
    private readonly IEndpointRouteBuilder _group;

    public QueryBuilder(IEndpointRouteBuilder group)
    {
        _group = group;
    }

    public RouteHandlerBuilder MapGet(string pattern)
    {
        return _group.MapGet(pattern, async ([AsParameters] TRequest request, QueryExecutor executor, CancellationToken ct) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request, ct);
            return TypedResults.Ok(result);
        });
    }

    public RouteHandlerBuilder MapGet<TResponse>(string pattern, Func<TResult, CancellationToken, TResponse> transformer)
    {
        return _group.MapGet(pattern, async ([AsParameters] TRequest request, QueryExecutor executor, CancellationToken ct) =>
        {
            var result = await executor.Execute<TRequest, TResult>(request, ct);
            return transformer(result, ct);
        });
    }
}
