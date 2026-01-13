using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Modules.Family.Domain.Repositories;

/// <summary>
/// Repository interface for Family aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// Extends ISpecificationRepository to support specification-based queries.
/// </summary>
public interface IFamilyRepository : ISpecificationRepository<FamilyAggregate, FamilyId>
{
    /// <summary>
    /// Gets the family that a user belongs to.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family if found; otherwise, null.</returns>
    /// <remarks>
    /// This method requires cross-module lookup via IUserLookupService.
    /// </remarks>
    [Obsolete("Use FindOneAsync with FamilyByOwnerSpecification or cross-module lookup.")]
    Task<FamilyAggregate?> GetFamilyByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the member count for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of members in the family.</returns>
    /// <remarks>
    /// This method requires cross-module lookup to Auth module.
    /// </remarks>
    [Obsolete("Use CountAsync with UsersByFamilySpecification via cross-module service.")]
    Task<int> GetMemberCountAsync(FamilyId familyId, CancellationToken cancellationToken = default);
}
