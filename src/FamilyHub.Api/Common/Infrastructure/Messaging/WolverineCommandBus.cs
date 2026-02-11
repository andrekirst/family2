using FamilyHub.Common.Application;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Wolverine implementation of ICommandBus.
/// Adapts Wolverine's IMessageBus to our domain abstraction.
/// </summary>
public sealed class WolverineCommandBus(Wolverine.IMessageBus messageBus) : ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        return await messageBus.InvokeAsync<TResult>(command, ct);
    }
}
