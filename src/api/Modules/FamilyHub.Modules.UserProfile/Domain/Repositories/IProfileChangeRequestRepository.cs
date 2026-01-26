using FamilyHub.Modules.UserProfile.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;

namespace FamilyHub.Modules.UserProfile.Domain.Repositories;

/// <summary>
/// Repository interface for ProfileChangeRequest aggregate root.
/// Follows DDD repository pattern - operates on aggregate roots only.
/// Extends ISpecificationRepository to support specification-based queries.
/// </summary>
public interface IProfileChangeRequestRepository : ISpecificationRepository<ProfileChangeRequest, ChangeRequestId>
{
    /// <summary>
    /// Gets all pending change requests for a family.
    /// Used by parents/admins to see the approval queue.
    /// </summary>
    /// <param name="familyId">The family ID to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending change requests for the family.</returns>
    Task<IReadOnlyList<ProfileChangeRequest>> GetPendingByFamilyAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending change requests made by a specific user.
    /// Used by children to see their pending changes.
    /// </summary>
    /// <param name="userId">The user ID who requested the changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending change requests by the user.</returns>
    Task<IReadOnlyList<ProfileChangeRequest>> GetPendingByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending change request for a specific profile field.
    /// Used to prevent duplicate pending requests for the same field.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending change request if exists; otherwise, null.</returns>
    Task<ProfileChangeRequest?> GetPendingByProfileAndFieldAsync(
        UserProfileId profileId,
        string fieldName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all change requests for a user, including approved and rejected.
    /// Used to show change history to the user.
    /// </summary>
    /// <param name="userId">The user ID who requested the changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all change requests by the user.</returns>
    Task<IReadOnlyList<ProfileChangeRequest>> GetAllByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);
}
