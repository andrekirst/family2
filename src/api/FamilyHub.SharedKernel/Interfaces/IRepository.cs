using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.SharedKernel.Interfaces;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
public interface IRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Checks if an entity with the specified identifier exists.
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
