using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Family.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFamilyMemberRepository.
/// </summary>
public sealed class FamilyMemberRepository(AppDbContext context) : IFamilyMemberRepository
{
    public async Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.UserId == userId && fm.FamilyId == familyId, cancellationToken);
    }

    public async Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.FamilyMembers
            .Include(fm => fm.User)
            .Where(fm => fm.FamilyId == familyId && fm.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FamilyMember member, CancellationToken cancellationToken = default)
    {
        await context.FamilyMembers.AddAsync(member, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
