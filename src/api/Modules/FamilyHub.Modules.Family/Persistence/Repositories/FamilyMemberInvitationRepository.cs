using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the FamilyMemberInvitation repository using FamilyDbContext.
///
/// PHASE 5 STATE: Repository now resides in Family module with its own DbContext.
///
/// CROSS-MODULE QUERIES:
/// - IsUserMemberOfFamilyAsync requires data from Auth module (User table)
/// - This query is handled via IUserLookupService abstraction
/// - This maintains bounded context separation while enabling necessary cross-module operations
/// </summary>
/// <param name="context">The Family module database context.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
public sealed class FamilyMemberInvitationRepository(FamilyDbContext context, IUserLookupService userLookupService) : IFamilyMemberInvitationRepository
{
    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .FirstOrDefaultAsync(
                i => i.FamilyId == familyId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        await context.FamilyMemberInvitations.AddAsync(invitation, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        // EF Core tracks changes automatically when entities are loaded from the context
        context.FamilyMemberInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        // Use IUserLookupService for cross-module query to Auth module
        return await userLookupService.IsEmailMemberOfFamilyAsync(familyId, email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMemberInvitations
            .Where(i => i.ExpiresAt < expirationThreshold && i.Status == InvitationStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveInvitations(List<FamilyMemberInvitation> invitations)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        context.FamilyMemberInvitations.RemoveRange(invitations);
    }
}
