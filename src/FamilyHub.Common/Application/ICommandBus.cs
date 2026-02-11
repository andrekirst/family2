namespace FamilyHub.Common.Application;

/// <summary>
/// Abstraction for sending commands through the messaging infrastructure.
/// Decouples domain logic from the underlying message bus implementation (Wolverine).
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Sends a command and awaits the result.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command</typeparam>
    /// <param name="command">The command to execute</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The command result</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}
