using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the FamilyMemberInvitation repository.
///
/// PHASE 3 STATE: This repository implements IFamilyMemberInvitationRepository from Family module
/// but remains in Auth module's Persistence layer for pragmatic reasons:
/// - Avoids circular dependency (Auth -> Family -> Auth)
/// - Shares AuthDbContext with other Auth repositories
/// - All entities remain in same "auth" schema
/// - Can query User table for membership checks
///
/// FUTURE: In Phase 5+, this will be moved to Family module when we introduce
/// a separate FamilyDbContext and resolve the cross-module database coupling.
/// </summary>
public sealed class FamilyMemberInvitationRepository(AuthDbContext context) : IFamilyMemberInvitationRepository
{
    /// <inheritdoc />
    public async Task<FamilyMemberInvitationAggregate?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitationAggregate?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitationAggregate>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitationAggregate?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(
                i => i.FamilyId == familyId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitationAggregate>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyMemberInvitationAggregate invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        await context.FamilyMemberInvitations.AddAsync(invitation, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(FamilyMemberInvitationAggregate invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        // EF Core tracks changes automatically when entities are loaded from the context
        context.FamilyMemberInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(
                u => u.FamilyId == familyId && u.Email == email,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitationAggregate>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.ExpiresAt < expirationThreshold && i.Status == InvitationStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveInvitations(List<FamilyMemberInvitationAggregate> invitations)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        context.FamilyMemberInvitations.RemoveRange(invitations);
    }
}
