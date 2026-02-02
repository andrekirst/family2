using Wolverine;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Wolverine implementation of IQueryBus.
/// Adapts Wolverine's IMessageBus to our domain abstraction.
/// </summary>
public sealed class WolverineQueryBus(IMessageBus messageBus) : Application.IQueryBus
{
    public async Task<TResult> QueryAsync<TResult>(Application.IQuery<TResult> query, CancellationToken ct = default)
    {
        return await messageBus.InvokeAsync<TResult>(query, ct);
    }
}
