using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Repositories;

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
    Task<FamilyDomain.Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the family that a user belongs to.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family if found; otherwise, null.</returns>
    Task<FamilyDomain.Family?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new family to the repository.
    /// </summary>
    /// <param name="family">The family to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(FamilyDomain.Family family, CancellationToken cancellationToken = default);
}
