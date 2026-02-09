using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Family.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFamilyMemberRepository.
/// </summary>
public sealed class FamilyMemberRepository(AppDbContext context) : IFamilyMemberRepository
{
    public async Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default)
    {
        return await context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.UserId == userId && fm.FamilyId == familyId, ct);
    }

    public async Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.FamilyMembers
            .Include(fm => fm.User)
            .Where(fm => fm.FamilyId == familyId && fm.IsActive)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FamilyMember member, CancellationToken ct = default)
    {
        await context.FamilyMembers.AddAsync(member, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
