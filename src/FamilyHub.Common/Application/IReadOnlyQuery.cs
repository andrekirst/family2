namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for read-only queries that do not modify state.
/// Queries implementing this interface will:
/// - Use AsNoTracking() for EF Core queries (via QueryAsNoTrackingBehavior)
/// - Skip SaveChangesAsync() in TransactionBehavior
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IReadOnlyQuery<out TResult> : IQuery<TResult>
{
}
