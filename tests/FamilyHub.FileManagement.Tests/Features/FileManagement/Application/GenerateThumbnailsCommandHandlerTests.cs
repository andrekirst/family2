using FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GenerateThumbnailsCommandHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_ImageFile_ShouldGenerateThumbnails()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/photo.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);
        storageProvider.SeedFile("files/photo.jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

        var command = new GenerateThumbnailsCommand(file.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(2);
        thumbnailRepo.Thumbnails.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NonImageFile_ShouldReturnZeroThumbnails()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("doc.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1000),
            StorageKey.From("files/doc.pdf"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);

        var command = new GenerateThumbnailsCommand(file.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(0);
        thumbnailRepo.Thumbnails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExistingThumbnails_ShouldSkipAlreadyGenerated()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.png"),
            MimeType.From("image/png"),
            FileSize.From(3000),
            StorageKey.From("files/photo.png"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);
        storageProvider.SeedFile("files/photo.png", new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        // Pre-seed existing 200x200 thumbnail
        var existingThumb = FileThumbnail.Create(
            file.Id, 200, 200, StorageKey.From("thumbnails/existing/200x200.webp"));
        thumbnailRepo.Thumbnails.Add(existingThumb);

        var command = new GenerateThumbnailsCommand(file.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ThumbnailsGenerated.Should().Be(1);
        thumbnailRepo.Thumbnails.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var command = new GenerateThumbnailsCommand(FileId.New(), FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/photo.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());
        fileRepo.Files.Add(file);

        var command = new GenerateThumbnailsCommand(file.Id, FamilyId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found in this family*");
    }

    [Fact]
    public async Task Handle_StorageEmpty_ShouldReturnFailure()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var thumbnailService = new FakeThumbnailGenerationService();
        var storageProvider = new FakeStorageProvider();
        var handler = new GenerateThumbnailsCommandHandler(
            fileRepo, thumbnailRepo, thumbnailService, storageProvider);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(5000),
            StorageKey.From("files/missing.jpg"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);
        // Do NOT seed storage — file is missing from storage

        var command = new GenerateThumbnailsCommand(file.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ThumbnailsGenerated.Should().Be(0);
    }
}
