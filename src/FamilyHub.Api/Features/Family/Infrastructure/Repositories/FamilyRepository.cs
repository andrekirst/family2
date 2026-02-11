using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFamilyRepository.
/// </summary>
public sealed class FamilyRepository(AppDbContext context) : IFamilyRepository
{
    public async Task<FamilyEntity?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<FamilyEntity?> GetByIdWithMembersAsync(FamilyId id, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FamilyEntity?> GetByOwnerIdAsync(UserId ownerId, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .FirstOrDefaultAsync(f => f.OwnerId == ownerId, cancellationToken);
    }

    public async Task<bool> UserHasFamilyAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await context.Families
            .AnyAsync(f => f.OwnerId == userId, cancellationToken);
    }

    public async Task AddAsync(FamilyEntity family, CancellationToken cancellationToken = default)
    {
        await context.Families.AddAsync(family, cancellationToken);
    }

    public Task UpdateAsync(FamilyEntity family, CancellationToken cancellationToken = default)
    {
        context.Families.Update(family);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
