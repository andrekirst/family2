using FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GenerateThumbnailsCommandHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_ImageFile_ShouldGenerateThumbnails()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/photo.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        thumbnailService.CanGenerateThumbnail("image/jpeg").Returns(true);
        storageProvider.DownloadAsync("files/photo.jpg", Arg.Any<CancellationToken>())
            .Returns(new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }));
        thumbnailService.GenerateThumbnailAsync(Arg.Any<byte[]>(), "image/jpeg", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new byte[] { 1, 2, 3 });
        storageProvider.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("thumb-key");
        thumbnailRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileThumbnail>());

        var command = new GenerateThumbnailsCommand(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(2);
        await thumbnailRepo.Received(2).AddAsync(Arg.Any<FileThumbnail>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonImageFile_ShouldReturnZeroThumbnails()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("files/doc.pdf"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        thumbnailService.CanGenerateThumbnail("application/pdf").Returns(false);

        var command = new GenerateThumbnailsCommand(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(0);
        await thumbnailRepo.DidNotReceive().AddAsync(Arg.Any<FileThumbnail>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingThumbnails_ShouldSkipAlreadyGenerated()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.png"),
            MimeType.From("image/png"),
            FileSize.From(3000),
            StorageKey.From("files/photo.png"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        thumbnailService.CanGenerateThumbnail("image/png").Returns(true);
        storageProvider.DownloadAsync("files/photo.png", Arg.Any<CancellationToken>())
            .Returns(new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 }));
        thumbnailService.GenerateThumbnailAsync(Arg.Any<byte[]>(), "image/png", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new byte[] { 1, 2, 3 });
        storageProvider.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("thumb-key");

        // Pre-seed existing 200x200 thumbnail — mock the per-size check the handler actually uses
        var existingThumb = FileThumbnail.Create(
            file.Id, 200, 200, StorageKey.From("thumbnails/existing/200x200.webp"), DateTimeOffset.UtcNow);
        thumbnailRepo.GetByFileIdAndSizeAsync(file.Id, 200, 200, Arg.Any<CancellationToken>())
            .Returns(existingThumb);
        thumbnailRepo.GetByFileIdAndSizeAsync(file.Id, 800, 800, Arg.Any<CancellationToken>())
            .Returns((FileThumbnail?)null);

        var command = new GenerateThumbnailsCommand(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(1);
        await thumbnailRepo.Received(1).AddAsync(Arg.Any<FileThumbnail>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new GenerateThumbnailsCommand(FileId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/photo.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new GenerateThumbnailsCommand(file.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found in this family*");
    }

    [Fact]
    public async Task Handle_StorageEmpty_ShouldReturnFailure()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var thumbnailService = Substitute.For<IThumbnailGenerationService>();
        var storageProvider = Substitute.For<IStorageProvider>();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider, TimeProvider.System);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/missing.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        thumbnailService.CanGenerateThumbnail("image/jpeg").Returns(true);
        storageProvider.DownloadAsync("files/missing.jpg", Arg.Any<CancellationToken>())
            .Returns((Stream?)null);

        var command = new GenerateThumbnailsCommand(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ThumbnailsGenerated.Should().Be(0);
    }
}
