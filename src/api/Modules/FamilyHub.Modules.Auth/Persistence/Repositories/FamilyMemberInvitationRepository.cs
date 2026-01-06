using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.Auth.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the FamilyMemberInvitation repository.
/// </summary>
public sealed class FamilyMemberInvitationRepository(AuthDbContext context) : IFamilyMemberInvitationRepository
{
    private readonly AuthDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetByTokenAsync(InvitationToken token, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyMemberInvitation?> GetPendingByEmailAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .FirstOrDefaultAsync(
                i => i.FamilyId == familyId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .Where(i => i.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        await _context.FamilyMemberInvitations.AddAsync(invitation, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(FamilyMemberInvitation invitation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invitation);

        // EF Core tracks changes automatically when entities are loaded from the context
        _context.FamilyMemberInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> IsUserMemberOfFamilyAsync(FamilyId familyId, Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(
                u => u.FamilyId == familyId && u.Email == email,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<FamilyMemberInvitation>> GetExpiredInvitationsForCleanupAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyMemberInvitations
            .Where(i => i.ExpiresAt < expirationThreshold && i.Status == InvitationStatus.Expired)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveInvitations(List<FamilyMemberInvitation> invitations)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        _context.FamilyMemberInvitations.RemoveRange(invitations);
    }
}
