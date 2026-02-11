namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for commands that return a result.
/// Commands represent write operations (state changes) in the application.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<out TResult>
{
}
