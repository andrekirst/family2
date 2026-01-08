using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Repositories;

/// <summary>
/// Repository for family member invitations.
/// </summary>
public interface IFamilyMemberInvitationRepository
{
    /// <summary>
    /// Gets an invitation by its ID.
    /// </summary>
    Task<FamilyMemberInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by its token.
    /// </summary>
    Task<FamilyMemberInvitation?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending invitations for a family.
    /// </summary>
    Task<List<FamilyMemberInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invitation by email for a specific family (for duplicate detection).
    /// </summary>
    Task<FamilyMemberInvitation?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invitations for a family (any status).
    /// </summary>
    Task<List<FamilyMemberInvitation>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new invitation.
    /// </summary>
    Task AddAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing invitation.
    /// </summary>
    Task UpdateAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is already a member of a family.
    /// </summary>
    Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired invitations older than the specified date (for cleanup).
    /// </summary>
    Task<List<FamilyMemberInvitation>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes invitations permanently.
    /// </summary>
    void RemoveInvitations(List<FamilyMemberInvitation> invitations);
}
