using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain.Repositories;

/// <summary>
/// Repository interface for Family aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// </summary>
public interface IFamilyRepository
{
    /// <summary>
    /// Gets a family by their unique identifier.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family if found; otherwise, null.</returns>
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all families that a user belongs to.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of families the user is a member of.</returns>
    Task<IReadOnlyList<Family>> GetFamiliesByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new family to the repository.
    /// </summary>
    /// <param name="family">The family to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Family family, CancellationToken cancellationToken = default);
}
