namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for queries that return a result.
/// Queries represent read operations (data retrieval) in the application.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<out TResult>
{
}
