namespace FamilyHub.Api.Common.Application;

/// <summary>
/// Interface for command handlers. Extends Mediator's ICommandHandler
/// for source-generated discovery while keeping our own abstraction layer.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
    : Mediator.ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    // Inherited: ValueTask<TResult> Handle(TCommand command, CancellationToken ct);
}
