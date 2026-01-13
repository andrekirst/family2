using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.Specifications;

namespace FamilyHub.SharedKernel.Interfaces;

/// <summary>
/// Repository interface that supports specification-based queries.
/// Extends IRepository with methods for finding, counting, and checking existence using specifications.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public interface ISpecificationRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Finds a single entity matching the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> FindOneAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity matching the specification, wrapped in Maybe&lt;T&gt;.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Maybe containing the entity if found, or None.</returns>
    Task<Maybe<TEntity>> FindOneMaybeAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    Task<List<TEntity>> FindAllAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any entity matches; otherwise, false.</returns>
    Task<bool> AnyAsync(
        IQueryableSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all entities matching the specification and projects to a result type.
    /// </summary>
    /// <typeparam name="TResult">The projected result type.</typeparam>
    /// <param name="specification">The projection specification to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of projected results.</returns>
    Task<List<TResult>> FindAllProjectedAsync<TResult>(
        IProjectionSpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default);
}
