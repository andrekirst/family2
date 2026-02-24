using FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class AddFileToAlbumCommandHandlerTests
{
    private static (AddFileToAlbumCommandHandler handler, FakeAlbumRepository albumRepo, FakeStoredFileRepository fileRepo, FakeAlbumItemRepository itemRepo) CreateHandler()
    {
        var albumRepo = new FakeAlbumRepository();
        var fileRepo = new FakeStoredFileRepository();
        var itemRepo = new FakeAlbumItemRepository();
        var handler = new AddFileToAlbumCommandHandler(albumRepo, fileRepo, itemRepo);
        return (handler, albumRepo, fileRepo, itemRepo);
    }

    private static StoredFile CreateTestFile(FamilyId familyId)
    {
        return StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldAddFileToAlbum()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, fileRepo, itemRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new AddFileToAlbumCommand(album.Id, file.Id, familyId, UserId.New());
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        itemRepo.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldAutoSetCoverImage()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, fileRepo, itemRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new AddFileToAlbumCommand(album.Id, file.Id, familyId, UserId.New());
        await handler.Handle(command, CancellationToken.None);

        album.CoverFileId.Should().Be(file.Id);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, fileRepo, itemRepo) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        // Pre-add the item
        itemRepo.Items.Add(AlbumItem.Create(album.Id, file.Id, UserId.New()));

        var command = new AddFileToAlbumCommand(album.Id, file.Id, familyId, UserId.New());
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        itemRepo.Items.Should().HaveCount(1); // No duplicate
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        var familyId = FamilyId.New();
        var (handler, _, fileRepo, _) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new AddFileToAlbumCommand(AlbumId.New(), file.Id, familyId, UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.AlbumNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var familyId = FamilyId.New();
        var (handler, albumRepo, _, _) = CreateHandler();

        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        albumRepo.Albums.Add(album);

        var command = new AddFileToAlbumCommand(album.Id, FileId.New(), familyId, UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }
}
