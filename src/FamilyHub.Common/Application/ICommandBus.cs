namespace FamilyHub.Common.Application;

/// <summary>
/// Abstraction for sending commands through the messaging infrastructure.
/// Decouples domain logic from the underlying message bus implementation.
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Sends a command and awaits the result.
    /// </summary>
    ValueTask<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}
