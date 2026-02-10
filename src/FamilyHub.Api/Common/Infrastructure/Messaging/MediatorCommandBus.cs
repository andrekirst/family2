using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Messaging;

/// <summary>
/// Mediator implementation of ICommandBus.
/// Adapts Mediator's IMediator.Send to our domain abstraction.
/// </summary>
public sealed class MediatorCommandBus(IMediator mediator) : Application.ICommandBus
{
    public ValueTask<TResult> SendAsync<TResult>(Application.ICommand<TResult> command, CancellationToken ct = default)
        => mediator.Send(command, ct);
}
