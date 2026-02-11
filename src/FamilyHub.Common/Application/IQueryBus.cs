namespace FamilyHub.Common.Application;

/// <summary>
/// Abstraction for executing queries through the messaging infrastructure.
/// Decouples domain logic from the underlying message bus implementation.
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Executes a query and returns the result.
    /// </summary>
    ValueTask<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
