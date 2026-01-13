using MediatR;

namespace FamilyHub.SharedKernel.Application.CQRS;

/// <summary>
/// Handler interface for queries that do not return a response.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
public interface IQueryHandler<in TQuery> : IRequestHandler<TQuery>
    where TQuery : IQuery;

/// <summary>
/// Handler interface for queries that return a response.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
