using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Family.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFamilyInvitationRepository.
/// </summary>
public sealed class FamilyInvitationRepository(AppDbContext context) : IFamilyInvitationRepository
{
    public async Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .FirstOrDefaultAsync(fi => fi.Id == id, cancellationToken);
    }

    public async Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken invitationToken, CancellationToken cancellationToken = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .FirstOrDefaultAsync(fi => fi.TokenHash == invitationToken, cancellationToken);
    }

    public async Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.InvitedByUser)
            .Where(fi => fi.FamilyId == familyId && fi.Status == InvitationStatus.Pending)
            .OrderByDescending(fi => fi.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyInvitations
            .FirstOrDefaultAsync(fi => fi.InviteeEmail == email && fi.FamilyId == familyId && fi.Status == InvitationStatus.Pending, cancellationToken);
    }

    public async Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .Where(fi => fi.InviteeEmail == email && fi.Status == InvitationStatus.Pending && fi.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(fi => fi.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FamilyInvitation invitation, CancellationToken cancellationToken = default)
    {
        await context.FamilyInvitations.AddAsync(invitation, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
