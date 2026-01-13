using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Modules.Family.Domain.Repositories;

/// <summary>
/// Repository for family member invitations.
/// Extends ISpecificationRepository to support specification-based queries.
/// </summary>
public interface IFamilyMemberInvitationRepository : ISpecificationRepository<FamilyMemberInvitationAggregate, InvitationId>
{
    /// <summary>
    /// Gets an invitation by its token.
    /// </summary>
    [Obsolete("Use FindOneAsync with InvitationByTokenSpecification.")]
    Task<FamilyMemberInvitationAggregate?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending invitations for a family.
    /// </summary>
    [Obsolete("Use FindAllAsync with PendingInvitationByFamilySpecification.")]
    Task<List<FamilyMemberInvitationAggregate>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pending invitation by email for a specific family (for duplicate detection).
    /// </summary>
    [Obsolete("Use FindOneAsync with PendingInvitationByEmailSpecification.")]
    Task<FamilyMemberInvitationAggregate?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invitations for a family (any status).
    /// </summary>
    [Obsolete("Use FindAllAsync with InvitationsByFamilySpecification (create if needed).")]
    Task<List<FamilyMemberInvitationAggregate>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing invitation.
    /// </summary>
    /// <remarks>
    /// Kept for EF Core change tracking scenarios where explicit update is needed.
    /// Consider using Update from base interface instead.
    /// </remarks>
    Task UpdateAsync(FamilyMemberInvitationAggregate invitation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is already a member of a family.
    /// </summary>
    /// <remarks>
    /// This method requires cross-module lookup to Auth module.
    /// </remarks>
    [Obsolete("Use cross-module service with UsersByFamilySpecification.")]
    Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired invitations older than the specified date (for cleanup).
    /// </summary>
    [Obsolete("Use FindAllAsync with ExpiredInvitationForCleanupSpecification.")]
    Task<List<FamilyMemberInvitationAggregate>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes invitations permanently.
    /// </summary>
    /// <remarks>
    /// Kept for batch removal scenarios. Consider using Remove from base interface for single items.
    /// </remarks>
    void RemoveInvitations(List<FamilyMemberInvitationAggregate> invitations);
}
