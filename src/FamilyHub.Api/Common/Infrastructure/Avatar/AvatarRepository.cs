using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// EF Core implementation of IAvatarRepository.
/// </summary>
public sealed class AvatarRepository(AppDbContext dbContext) : IAvatarRepository
{
    public async Task<AvatarAggregate?> GetByIdAsync(AvatarId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Avatars
            .Include(a => a.Variants)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(AvatarId id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Avatars.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(AvatarAggregate avatar, CancellationToken cancellationToken = default)
    {
        await dbContext.Avatars.AddAsync(avatar, cancellationToken);
    }

    public async Task DeleteAsync(AvatarId id, CancellationToken cancellationToken = default)
    {
        var avatar = await dbContext.Avatars
            .Include(a => a.Variants)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (avatar is not null)
        {
            dbContext.Avatars.Remove(avatar);
        }
    }
}
