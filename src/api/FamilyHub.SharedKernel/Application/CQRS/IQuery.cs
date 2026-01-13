using MediatR;

namespace FamilyHub.SharedKernel.Application.CQRS;

/// <summary>
/// Marker interface for queries (read operations that do not change system state).
/// Queries represent requests to retrieve data without side effects.
/// </summary>
/// <remarks>
/// All query classes should implement this interface (or <see cref="IQuery{TResponse}"/>)
/// to enable type-safe CQRS pattern enforcement through architecture tests.
/// </remarks>
public interface IQuery : IRequest;

/// <summary>
/// Marker interface for queries that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
/// <remarks>
/// Queries should return data types or projections.
/// Example: <c>GetUserFamiliesQuery : IQuery&lt;GetUserFamiliesResult&gt;</c>
/// </remarks>
public interface IQuery<TResponse> : IRequest<TResponse>, IQuery;
