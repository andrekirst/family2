using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeUserFavoriteRepository : IUserFavoriteRepository
{
    public List<UserFavorite> Favorites { get; } = [];

    public Task<List<UserFavorite>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => Task.FromResult(Favorites.Where(uf => uf.UserId == userId).OrderByDescending(uf => uf.FavoritedAt).ToList());

    public Task<bool> ExistsAsync(UserId userId, FileId fileId, CancellationToken ct = default)
        => Task.FromResult(Favorites.Any(uf => uf.UserId == userId && uf.FileId == fileId));

    public Task AddAsync(UserFavorite favorite, CancellationToken ct = default)
    {
        Favorites.Add(favorite);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(UserFavorite favorite, CancellationToken ct = default)
    {
        var existing = Favorites.FirstOrDefault(uf => uf.UserId == favorite.UserId && uf.FileId == favorite.FileId);
        if (existing is not null) Favorites.Remove(existing);
        return Task.CompletedTask;
    }
}
