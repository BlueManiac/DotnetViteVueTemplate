namespace Persistence.Shared.Cqrs;

public interface IQuery<TRequest, TResult>
{
    Task<TResult> ExecuteAsync(TRequest request, CancellationToken ct);
}
