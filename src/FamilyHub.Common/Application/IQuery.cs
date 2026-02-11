namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for queries that return a result.
/// Queries represent read operations (data retrieval) in the application.
/// Extends Mediator's IQuery for source-generated handler discovery.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<out TResult> : Mediator.IQuery<TResult>
{
}
