namespace FamilyHub.Common.Application;

/// <summary>
/// Abstraction for executing queries through the messaging infrastructure.
/// Decouples domain logic from the underlying message bus implementation (Wolverine).
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Executes a query and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the query</typeparam>
    /// <param name="query">The query to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The query result</returns>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
