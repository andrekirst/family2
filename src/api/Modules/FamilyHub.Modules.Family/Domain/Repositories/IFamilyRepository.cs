using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

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
    Task<FamilyAggregate?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the family that a user belongs to.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family if found; otherwise, null.</returns>
    Task<FamilyAggregate?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the member count for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of members in the family.</returns>
    Task<int> GetMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new family to the repository.
    /// </summary>
    /// <param name="family">The family to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(FamilyAggregate family, CancellationToken cancellationToken = default);
}
