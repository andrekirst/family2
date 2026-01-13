using MediatR;

namespace FamilyHub.SharedKernel.Application.CQRS;

/// <summary>
/// Marker interface for commands (write operations that change system state).
/// Commands represent intentions to perform actions that modify data.
/// </summary>
/// <remarks>
/// All command classes should implement this interface (or <see cref="ICommand{TResponse}"/>)
/// to enable type-safe CQRS pattern enforcement through architecture tests.
/// </remarks>
public interface ICommand : IRequest;

/// <summary>
/// Marker interface for commands that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <remarks>
/// Commands should return result types that indicate success/failure status.
/// Example: <c>CreateFamilyCommand : ICommand&lt;CreateFamilyResult&gt;</c>
/// </remarks>
public interface ICommand<TResponse> : IRequest<TResponse>, ICommand;
