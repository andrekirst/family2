using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameAlbumCommandHandlerTests
{
    private static (RenameAlbumCommandHandler handler, FakeAlbumRepository albumRepo) CreateHandler()
    {
        var albumRepo = new FakeAlbumRepository();
        var handler = new RenameAlbumCommandHandler(albumRepo);
        return (handler, albumRepo);
    }

    [Fact]
    public async Task Handle_ShouldRenameAlbum()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Old Name"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var command = new RenameAlbumCommand(album.Id, AlbumName.From("New Name"), familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.AlbumId.Should().Be(album.Id);
        album.Name.Value.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new RenameAlbumCommand(AlbumId.New(), AlbumName.From("New"), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.AlbumNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumBelongsToDifferentFamily()
    {
        var (handler, albumRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, FamilyId.New(), UserId.New());
        albumRepo.Albums.Add(album);

        var command = new RenameAlbumCommand(album.Id, AlbumName.From("New"), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
