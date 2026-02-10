namespace FamilyHub.Api.Common.Application;

/// <summary>
/// Marker interface for commands that return a result.
/// Commands represent write operations (state changes) in the application.
/// Extends Mediator's ICommand for source-generated handler discovery.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<out TResult> : Mediator.ICommand<TResult>
{
}
