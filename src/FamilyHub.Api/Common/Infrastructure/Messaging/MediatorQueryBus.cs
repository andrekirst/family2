using FamilyHub.Common.Application;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Mediator implementation of IQueryBus.
/// Adapts Mediator's IMediator.Send to our domain abstraction.
/// </summary>
public sealed class MediatorQueryBus(Mediator.IMediator mediator) : IQueryBus
{
    public ValueTask<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
        => mediator.Send(query, ct);
}
