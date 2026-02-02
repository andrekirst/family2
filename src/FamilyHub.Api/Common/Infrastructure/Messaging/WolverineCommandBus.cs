using Wolverine;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Wolverine implementation of ICommandBus.
/// Adapts Wolverine's IMessageBus to our domain abstraction.
/// </summary>
public sealed class WolverineCommandBus : global::FamilyHub.Api.Common.Application.ICommandBus
{
    private readonly IMessageBus _messageBus;

    public WolverineCommandBus(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task<TResult> SendAsync<TResult>(global::FamilyHub.Api.Common.Application.ICommand<TResult> command, CancellationToken ct = default)
    {
        return await _messageBus.InvokeAsync<TResult>(command, ct);
    }
}
