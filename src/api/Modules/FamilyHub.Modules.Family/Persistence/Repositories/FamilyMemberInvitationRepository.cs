using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Family.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the FamilyMemberInvitation repository.
/// </summary>
public sealed class FamilyMemberInvitationRepository(FamilyDbContext context) : IFamilyMemberInvitationRepository
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
        // NOTE: This query needs to check the auth.users table
        // Since we don't have User entity in Family module, we use raw SQL with FormattableString
        FormattableString sql = $@"
            SELECT EXISTS(
                SELECT 1 FROM auth.users
                WHERE family_id = {familyId.Value} AND email = {email.Value}
            )";

        return await context.Database
            .SqlQuery<bool>(sql)
            .FirstOrDefaultAsync(cancellationToken);
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
