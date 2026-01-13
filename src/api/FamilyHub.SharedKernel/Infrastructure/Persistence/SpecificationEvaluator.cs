using System.Diagnostics;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Evaluates specifications against EF Core IQueryable sources.
/// Applies filtering, includes, ordering, and pagination.
/// </summary>
public static class SpecificationEvaluator
{
    private static readonly DiagnosticSource DiagnosticSource =
        new DiagnosticListener(SpecificationDiagnosticEvents.ListenerName);

    /// <summary>
    /// Applies a specification to an IQueryable source.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="inputQuery">The source queryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The queryable with specification applied.</returns>
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        IQueryableSpecification<T> specification)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(inputQuery);
        ArgumentNullException.ThrowIfNull(specification);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var query = inputQuery;

            // Apply ignore query filters if specified (for soft-delete override)
            if (specification.IgnoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            // Apply expression-based includes
            foreach (var include in specification.Includes)
            {
                query = query.Include(include);
            }

            // Apply string-based includes (for ThenInclude scenarios)
            foreach (var includeString in specification.IncludeStrings)
            {
                query = query.Include(includeString);
            }

            // Apply where clause
            query = query.Where(specification.ToExpression());

            // Apply ordering if applicable
            if (specification is IOrderedSpecification<T> orderedSpec && orderedSpec.OrderExpressions.Count > 0)
            {
                query = ApplyOrdering(query, orderedSpec);
            }

            // Apply pagination if applicable
            if (specification is IPaginatedSpecification<T> paginatedSpec)
            {
                query = query.Skip(paginatedSpec.Skip).Take(paginatedSpec.Take);
            }

            stopwatch.Stop();
            EmitEvaluatedEvent(specification, stopwatch.Elapsed);

            return query;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EmitFailedEvent(specification, ex);
            throw;
        }
    }

    /// <summary>
    /// Applies a projection specification to an IQueryable source.
    /// </summary>
    /// <typeparam name="T">The source entity type.</typeparam>
    /// <typeparam name="TResult">The projected result type.</typeparam>
    /// <param name="inputQuery">The source queryable.</param>
    /// <param name="specification">The projection specification to apply.</param>
    /// <returns>The queryable with specification and projection applied.</returns>
    public static IQueryable<TResult> GetProjectedQuery<T, TResult>(
        IQueryable<T> inputQuery,
        IProjectionSpecification<T, TResult> specification)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = GetQuery(inputQuery, specification);
        return query.Select(specification.Selector);
    }

    /// <summary>
    /// Applies a specification and returns Maybe&lt;T&gt; for single-entity queries.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="inputQuery">The source queryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Maybe containing the entity if found, or None.</returns>
    public static async Task<Domain.Maybe<T>> GetMaybeAsync<T>(
        IQueryable<T> inputQuery,
        IQueryableSpecification<T> specification,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var query = GetQuery(inputQuery, specification);
        var result = await query.FirstOrDefaultAsync(cancellationToken);
        return Domain.Maybe<T>.From(result);
    }

    private static IQueryable<T> ApplyOrdering<T>(
        IQueryable<T> query,
        IOrderedSpecification<T> orderedSpec)
        where T : class
    {
        var orderExpressions = orderedSpec.OrderExpressions;

        if (orderExpressions.Count == 0)
        {
            return query;
        }

        var first = orderExpressions[0];
        var orderedQuery = first.IsDescending
            ? query.OrderByDescending(first.KeySelector)
            : query.OrderBy(first.KeySelector);

        for (var i = 1; i < orderExpressions.Count; i++)
        {
            var orderExpr = orderExpressions[i];
            orderedQuery = orderExpr.IsDescending
                ? orderedQuery.ThenByDescending(orderExpr.KeySelector)
                : orderedQuery.ThenBy(orderExpr.KeySelector);
        }

        return orderedQuery;
    }

    private static void EmitEvaluatedEvent<T>(
        IQueryableSpecification<T> specification,
        TimeSpan elapsed)
        where T : class
    {
        if (DiagnosticSource.IsEnabled(SpecificationDiagnosticEvents.SpecificationEvaluated))
        {
            var data = new SpecificationEvaluatedData(
                SpecificationType: specification.GetType().Name,
                EntityType: typeof(T).Name,
                ElapsedMilliseconds: elapsed.TotalMilliseconds,
                IgnoredQueryFilters: specification.IgnoreQueryFilters,
                IncludeCount: specification.Includes.Count + specification.IncludeStrings.Count,
                HasOrdering: specification is IOrderedSpecification<T>,
                HasPagination: specification is IPaginatedSpecification<T>,
                Timestamp: DateTime.UtcNow);

            DiagnosticSource.Write(SpecificationDiagnosticEvents.SpecificationEvaluated, data);
        }
    }

    private static void EmitFailedEvent<T>(
        IQueryableSpecification<T> specification,
        Exception exception)
        where T : class
    {
        if (DiagnosticSource.IsEnabled(SpecificationDiagnosticEvents.SpecificationFailed))
        {
            var data = new SpecificationFailedData(
                SpecificationType: specification.GetType().Name,
                EntityType: typeof(T).Name,
                ErrorMessage: exception.Message,
                ExceptionType: exception.GetType().Name,
                Timestamp: DateTime.UtcNow);

            DiagnosticSource.Write(SpecificationDiagnosticEvents.SpecificationFailed, data);
        }
    }
}
