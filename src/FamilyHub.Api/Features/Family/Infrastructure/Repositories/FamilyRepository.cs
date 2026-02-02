using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFamilyRepository.
/// </summary>
public sealed class FamilyRepository(AppDbContext context) : IFamilyRepository
{
    public async Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken ct = default)
    {
        return await context.Families.FindAsync([id], cancellationToken: ct);
    }

    public async Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken ct = default)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken ct = default)
    {
        return await context.Families
            .FirstOrDefaultAsync(f => f.OwnerId == ownerId, ct);
    }

    public async Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken ct = default)
    {
        return await context.Families
            .AnyAsync(f => f.OwnerId == userId, ct);
    }

    public async Task AddAsync(FamilyEntity family, CancellationToken ct = default)
    {
        await context.Families.AddAsync(family, ct);
    }

    public Task UpdateAsync(FamilyEntity family, CancellationToken ct = default)
    {
        context.Families.Update(family);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
