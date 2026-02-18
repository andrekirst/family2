using FamilyHub.Api.Features.FileManagement.Application.Queries.GetAlbums;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetAlbumsQueryHandlerTests
{
    private static (GetAlbumsQueryHandler handler, FakeAlbumRepository albumRepo, FakeAlbumItemRepository itemRepo) CreateHandler()
    {
        var albumRepo = new FakeAlbumRepository();
        var itemRepo = new FakeAlbumItemRepository();
        var handler = new GetAlbumsQueryHandler(albumRepo, itemRepo);
        return (handler, albumRepo, itemRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnAlbumsWithItemCounts()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, itemRepo) = CreateHandler();

        var album1 = Album.Create(AlbumName.From("Summer"), null, familyId, UserId.New());
        var album2 = Album.Create(AlbumName.From("Winter"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album1);
        albumRepo.Albums.Add(album2);

        itemRepo.Items.Add(AlbumItem.Create(album1.Id, FileId.New(), UserId.New()));
        itemRepo.Items.Add(AlbumItem.Create(album1.Id, FileId.New(), UserId.New()));
        itemRepo.Items.Add(AlbumItem.Create(album2.Id, FileId.New(), UserId.New()));

        var query = new GetAlbumsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First(a => a.Name == "Summer").ItemCount.Should().Be(2);
        result.First(a => a.Name == "Winter").ItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoAlbums()
    {
        var (handler, _, _) = CreateHandler();

        var query = new GetAlbumsQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnAlbumsForRequestedFamily()
    {
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var (handler, albumRepo, _) = CreateHandler();

        albumRepo.Albums.Add(Album.Create(AlbumName.From("Mine"), null, familyId, UserId.New()));
        albumRepo.Albums.Add(Album.Create(AlbumName.From("Other"), null, otherFamilyId, UserId.New()));

        var query = new GetAlbumsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Mine");
    }
}
