namespace Persistence.Shared.Cqrs;

public interface IQuery<TRequest, TResult>
{
    TResult Execute(TRequest request, CancellationToken ct);
}
