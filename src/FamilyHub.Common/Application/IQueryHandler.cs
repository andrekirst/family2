namespace FamilyHub.Common.Application;

/// <summary>
/// Interface for query handlers. Extends Mediator's IQueryHandler
/// for source-generated discovery while keeping our own abstraction layer.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
    : Mediator.IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    // Inherited: ValueTask<TResult> Handle(TQuery query, CancellationToken ct);
}
