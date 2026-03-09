using FamilyHub.Api.Features.FileManagement.Application.Queries.GetAlbums;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetAlbumsQueryHandlerTests
{
    private readonly IAlbumRepository _albumRepo = Substitute.For<IAlbumRepository>();
    private readonly IAlbumItemRepository _itemRepo = Substitute.For<IAlbumItemRepository>();
    private readonly GetAlbumsQueryHandler _handler;

    public GetAlbumsQueryHandlerTests()
    {
        _handler = new GetAlbumsQueryHandler(_albumRepo, _itemRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnAlbumsWithItemCounts()
    {
        var familyId = FamilyId.New();

        var album1 = Album.Create(AlbumName.From("Summer"), null, familyId, UserId.New());
        var album2 = Album.Create(AlbumName.From("Winter"), null, familyId, UserId.New());
        _albumRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([album1, album2]);

        _itemRepo.GetItemCountAsync(album1.Id, Arg.Any<CancellationToken>()).Returns(2);
        _itemRepo.GetItemCountAsync(album2.Id, Arg.Any<CancellationToken>()).Returns(1);
        _itemRepo.GetFirstImageFileIdAsync(AlbumId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((FileId?)null);

        var query = new GetAlbumsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First(a => a.Name == "Summer").ItemCount.Should().Be(2);
        result.First(a => a.Name == "Winter").ItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoAlbums()
    {
        _albumRepo.GetByFamilyIdAsync(FamilyId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<Album>());

        var query = new GetAlbumsQuery()
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnAlbumsForRequestedFamily()
    {
        var familyId = FamilyId.New();

        var album = Album.Create(AlbumName.From("Mine"), null, familyId, UserId.New());
        _albumRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([album]);
        _itemRepo.GetItemCountAsync(album.Id, Arg.Any<CancellationToken>()).Returns(0);
        _itemRepo.GetFirstImageFileIdAsync(album.Id, Arg.Any<CancellationToken>())
            .Returns((FileId?)null);

        var query = new GetAlbumsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Mine");
    }
}
