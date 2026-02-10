using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Mediator implementation of IQueryBus.
/// Adapts Mediator's IMediator.Send to our domain abstraction.
/// </summary>
public sealed class MediatorQueryBus(IMediator mediator) : Application.IQueryBus
{
    public ValueTask<TResult> QueryAsync<TResult>(Application.IQuery<TResult> query, CancellationToken ct = default)
        => mediator.Send(query, ct);
}
