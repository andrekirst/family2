namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Base specification interface for in-memory evaluation of business rules.
/// Specifications encapsulate domain knowledge into reusable, composable, and testable units.
/// </summary>
/// <typeparam name="T">The entity type to evaluate.</typeparam>
public interface ISpecification<in T>
{
    /// <summary>
    /// Evaluates whether the entity satisfies this specification synchronously.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns>True if the entity satisfies the specification; otherwise, false.</returns>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Evaluates whether the entity satisfies this specification asynchronously.
    /// Use this overload when the specification requires service calls or async operations.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the entity satisfies the specification; otherwise, false.</returns>
    Task<bool> IsSatisfiedByAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates the specification and returns a Result with error context if evaluation fails.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns>A Result containing the evaluation outcome or error information.</returns>
    Result<bool> Evaluate(T entity);

    /// <summary>
    /// Evaluates the specification asynchronously and returns a Result with error context.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A Result containing the evaluation outcome or error information.</returns>
    Task<Result<bool>> EvaluateAsync(T entity, CancellationToken cancellationToken = default);
}
