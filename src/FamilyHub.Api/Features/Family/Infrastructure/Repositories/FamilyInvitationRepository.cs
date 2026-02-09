using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Domain.ValueObjects;
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
    public async Task<FamilyInvitation?> GetByIdAsync(InvitationId id, CancellationToken ct = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .FirstOrDefaultAsync(fi => fi.Id == id, ct);
    }

    public async Task<FamilyInvitation?> GetByTokenHashAsync(InvitationToken tokenHash, CancellationToken ct = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .FirstOrDefaultAsync(fi => fi.TokenHash == tokenHash, ct);
    }

    public async Task<List<FamilyInvitation>> GetPendingByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.InvitedByUser)
            .Where(fi => fi.FamilyId == familyId && fi.Status == InvitationStatus.Pending)
            .OrderByDescending(fi => fi.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<FamilyInvitation?> GetByEmailAndFamilyAsync(Email email, FamilyId familyId, CancellationToken ct = default)
    {
        return await context.FamilyInvitations
            .FirstOrDefaultAsync(fi => fi.InviteeEmail == email && fi.FamilyId == familyId && fi.Status == InvitationStatus.Pending, ct);
    }

    public async Task<List<FamilyInvitation>> GetPendingByEmailAsync(Email email, CancellationToken ct = default)
    {
        return await context.FamilyInvitations
            .Include(fi => fi.Family)
            .Include(fi => fi.InvitedByUser)
            .Where(fi => fi.InviteeEmail == email && fi.Status == InvitationStatus.Pending && fi.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(fi => fi.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FamilyInvitation invitation, CancellationToken ct = default)
    {
        await context.FamilyInvitations.AddAsync(invitation, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
