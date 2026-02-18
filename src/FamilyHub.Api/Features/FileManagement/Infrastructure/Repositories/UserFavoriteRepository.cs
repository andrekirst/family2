using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class UserFavoriteRepository(AppDbContext context) : IUserFavoriteRepository
{
    public async Task<List<UserFavorite>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await context.Set<UserFavorite>()
            .Where(uf => uf.UserId == userId)
            .OrderByDescending(uf => uf.FavoritedAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(UserId userId, FileId fileId, CancellationToken ct = default)
        => await context.Set<UserFavorite>()
            .AnyAsync(uf => uf.UserId == userId && uf.FileId == fileId, ct);

    public async Task AddAsync(UserFavorite favorite, CancellationToken ct = default)
        => await context.Set<UserFavorite>().AddAsync(favorite, ct);

    public Task RemoveAsync(UserFavorite favorite, CancellationToken ct = default)
    {
        context.Set<UserFavorite>().Remove(favorite);
        return Task.CompletedTask;
    }
}
