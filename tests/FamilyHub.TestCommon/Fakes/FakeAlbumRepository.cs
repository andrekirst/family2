using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeAlbumRepository : IAlbumRepository
{
    public List<Album> Albums { get; } = [];

    public Task<Album?> GetByIdAsync(AlbumId id, CancellationToken ct = default)
        => Task.FromResult(Albums.FirstOrDefault(a => a.Id == id));

    public Task<List<Album>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Albums.Where(a => a.FamilyId == familyId).OrderBy(a => a.Name).ToList());

    public Task AddAsync(Album album, CancellationToken ct = default)
    {
        Albums.Add(album);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Album album, CancellationToken ct = default)
    {
        Albums.Remove(album);
        return Task.CompletedTask;
    }
}
