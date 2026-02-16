using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// EF Core implementation of IAvatarRepository.
/// </summary>
public sealed class AvatarRepository(AppDbContext dbContext) : IAvatarRepository
{
    public async Task<AvatarAggregate?> GetByIdAsync(AvatarId id, CancellationToken ct = default)
    {
        return await dbContext.Avatars
            .Include(a => a.Variants)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task AddAsync(AvatarAggregate avatar, CancellationToken ct = default)
    {
        await dbContext.Avatars.AddAsync(avatar, ct);
    }

    public async Task DeleteAsync(AvatarId id, CancellationToken ct = default)
    {
        var avatar = await dbContext.Avatars
            .Include(a => a.Variants)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (avatar is not null)
        {
            dbContext.Avatars.Remove(avatar);
        }
    }
}
