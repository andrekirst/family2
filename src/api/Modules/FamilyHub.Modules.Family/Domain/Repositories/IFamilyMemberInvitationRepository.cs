using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Modules.Family.Domain.Repositories;

/// <summary>
/// Repository for family member invitations.
/// </summary>
public interface IFamilyMemberInvitationRepository
{
    /// <summary>
    /// Gets an invitation by its ID.
    /// </summary>
    Task<FamilyMemberInvitationAggregate?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invitation by its token.
    /// </summary>
    Task<FamilyMemberInvitationAggregate?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending invitations for a family.
    /// </summary>
    Task<List<FamilyMemberInvitationAggregate>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invitation by email for a specific family (for duplicate detection).
    /// </summary>
    Task<FamilyMemberInvitationAggregate?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invitations for a family (any status).
    /// </summary>
    Task<List<FamilyMemberInvitationAggregate>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new invitation.
    /// </summary>
    Task AddAsync(FamilyMemberInvitationAggregate invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing invitation.
    /// </summary>
    Task UpdateAsync(FamilyMemberInvitationAggregate invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is already a member of a family.
    /// </summary>
    Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired invitations older than the specified date (for cleanup).
    /// </summary>
    Task<List<FamilyMemberInvitationAggregate>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes invitations permanently.
    /// </summary>
    void RemoveInvitations(List<FamilyMemberInvitationAggregate> invitations);
}
