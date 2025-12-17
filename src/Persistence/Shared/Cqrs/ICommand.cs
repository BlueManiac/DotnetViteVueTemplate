namespace Persistence.Shared.Cqrs;

/// <summary>
/// Simple command with no return value
/// </summary>
public interface ICommand<TRequest>
{
    Task ExecuteAsync(TRequest request, CancellationToken ct);
}

/// <summary>
/// Command that returns a result
/// </summary>
public interface ICommand<TRequest, TResult> where TRequest : notnull
{
    Task<TResult> ExecuteAsync(TRequest request, CancellationToken ct);
}