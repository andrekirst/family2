using Wolverine;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Wolverine implementation of ICommandBus.
/// Adapts Wolverine's IMessageBus to our domain abstraction.
/// </summary>
public sealed class WolverineCommandBus(IMessageBus messageBus) : Application.ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(Application.ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        return await messageBus.InvokeAsync<TResult>(command, cancellationToken);
    }
}
