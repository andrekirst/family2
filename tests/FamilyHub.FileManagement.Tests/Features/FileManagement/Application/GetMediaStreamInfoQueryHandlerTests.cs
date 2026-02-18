using FamilyHub.Api.Features.FileManagement.Application.Queries.GetMediaStreamInfo;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetMediaStreamInfoQueryHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_StreamableFile_ShouldReturnStreamableInfo()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("video.mp4"),
            MimeType.From("video/mp4"),
            FileSize.From(10_000_000),
            StorageKey.From("files/video.mp4"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);

        var query = new GetMediaStreamInfoQuery(file.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.FileId.Should().Be(file.Id.Value);
        result.MimeType.Should().Be("video/mp4");
        result.FileSize.Should().Be(10_000_000);
        result.IsStreamable.Should().BeTrue();
        result.SupportsRangeRequests.Should().BeTrue();
        result.Thumbnails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonStreamableFile_ShouldReturnNonStreamable()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

        var familyId = FamilyId.New();
        var file = StoredFile.Create(
            FileName.From("doc.docx"),
            MimeType.From("application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
            FileSize.From(50_000),
            StorageKey.From("files/doc.docx"),
            Checksum.From(ValidChecksum),
            FolderId.New(),
            familyId,
            UserId.New());
        fileRepo.Files.Add(file);

        var query = new GetMediaStreamInfoQuery(file.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsStreamable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithThumbnails_ShouldIncludeThumbnailDtos()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

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

        thumbnailRepo.Thumbnails.Add(FileThumbnail.Create(
            file.Id, 200, 200, StorageKey.From("thumbs/200x200.webp")));
        thumbnailRepo.Thumbnails.Add(FileThumbnail.Create(
            file.Id, 800, 800, StorageKey.From("thumbs/800x800.webp")));

        var query = new GetMediaStreamInfoQuery(file.Id, familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Thumbnails.Should().HaveCount(2);
        result.Thumbnails[0].Width.Should().Be(200);
        result.Thumbnails[1].Width.Should().Be(800);
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

        var query = new GetMediaStreamInfoQuery(FileId.New(), FamilyId.New());

        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var fileRepo = new FakeStoredFileRepository();
        var thumbnailRepo = new FakeFileThumbnailRepository();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

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

        var query = new GetMediaStreamInfoQuery(file.Id, FamilyId.New());

        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*File not found in this family*");
    }
}
