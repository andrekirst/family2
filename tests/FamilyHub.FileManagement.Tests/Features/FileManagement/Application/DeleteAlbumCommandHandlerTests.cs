using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteAlbumCommandHandlerTests
{
    private static (DeleteAlbumCommandHandler handler, FakeAlbumRepository albumRepo, FakeAlbumItemRepository itemRepo) CreateHandler()
    {
        var albumRepo = new FakeAlbumRepository();
        var itemRepo = new FakeAlbumItemRepository();
        var handler = new DeleteAlbumCommandHandler(albumRepo, itemRepo);
        return (handler, albumRepo, itemRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAlbum()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, _) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var command = new DeleteAlbumCommand(album.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        albumRepo.Albums.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRemoveAlbumItems()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, itemRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        itemRepo.Items.Add(AlbumItem.Create(album.Id, FileId.New(), UserId.New()));
        itemRepo.Items.Add(AlbumItem.Create(album.Id, FileId.New(), UserId.New()));

        var command = new DeleteAlbumCommand(album.Id, familyId);
        await handler.Handle(command, CancellationToken.None);

        itemRepo.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        var (handler, _, _) = CreateHandler();

        var command = new DeleteAlbumCommand(AlbumId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.AlbumNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumBelongsToDifferentFamily()
    {
        var (handler, albumRepo, _) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, FamilyId.New(), UserId.New());
        albumRepo.Albums.Add(album);

        var command = new DeleteAlbumCommand(album.Id, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
