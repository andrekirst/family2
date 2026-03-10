using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class UserFavoriteRepository(AppDbContext context) : IUserFavoriteRepository
{
    public async Task<List<UserFavorite>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        => await context.Set<UserFavorite>()
            .Where(uf => uf.UserId == userId)
            .OrderByDescending(uf => uf.FavoritedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(UserId userId, FileId fileId, CancellationToken cancellationToken = default)
        => await context.Set<UserFavorite>()
            .AnyAsync(uf => uf.UserId == userId && uf.FileId == fileId, cancellationToken);

    public async Task AddAsync(UserFavorite favorite, CancellationToken cancellationToken = default)
        => await context.Set<UserFavorite>().AddAsync(favorite, cancellationToken);

    public Task RemoveAsync(UserFavorite favorite, CancellationToken cancellationToken = default)
    {
        context.Set<UserFavorite>().Remove(favorite);
        return Task.CompletedTask;
    }
}
