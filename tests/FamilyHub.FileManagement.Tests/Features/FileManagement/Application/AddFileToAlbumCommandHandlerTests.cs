using FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class AddFileToAlbumCommandHandlerTests
{
    private readonly IAlbumRepository _albumRepo = Substitute.For<IAlbumRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IAlbumItemRepository _itemRepo = Substitute.For<IAlbumItemRepository>();
    private readonly AddFileToAlbumCommandHandler _handler;

    public AddFileToAlbumCommandHandlerTests()
    {
        _handler = new AddFileToAlbumCommandHandler(_albumRepo, _fileRepo, _itemRepo);
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
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        var file = CreateTestFile(familyId);

        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _itemRepo.ExistsAsync(album.Id, file.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddFileToAlbumCommand(album.Id, file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _itemRepo.Received(1).AddAsync(Arg.Any<AlbumItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAutoSetCoverImage()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        var file = CreateTestFile(familyId);

        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _itemRepo.ExistsAsync(album.Id, file.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddFileToAlbumCommand(album.Id, file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        await _handler.Handle(command, CancellationToken.None);

        album.CoverFileId.Should().Be(file.Id);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());
        var file = CreateTestFile(familyId);

        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _itemRepo.ExistsAsync(album.Id, file.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = new AddFileToAlbumCommand(album.Id, file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _itemRepo.DidNotReceive().AddAsync(Arg.Any<AlbumItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        var familyId = FamilyId.New();
        var file = CreateTestFile(familyId);

        _albumRepo.GetByIdAsync(AlbumId.New(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs((Album?)null);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new AddFileToAlbumCommand(AlbumId.New(), file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.AlbumNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New());

        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);
        _fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs((StoredFile?)null);

        var command = new AddFileToAlbumCommand(album.Id, FileId.New())
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }
}
