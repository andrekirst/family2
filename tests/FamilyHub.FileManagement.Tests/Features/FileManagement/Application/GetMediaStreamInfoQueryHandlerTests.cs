using FamilyHub.Api.Features.FileManagement.Application.Queries.GetMediaStreamInfo;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetMediaStreamInfoQueryHandlerTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public async Task Handle_StreamableFile_ShouldReturnStreamableInfo()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
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
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        thumbnailRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileThumbnail>());

        var query = new GetMediaStreamInfoQuery(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.FileId.Should().Be(file.Id.Value);
        result.Value.MimeType.Should().Be("video/mp4");
        result.Value.FileSize.Should().Be(10_000_000);
        result.Value.IsStreamable.Should().BeTrue();
        result.Value.SupportsRangeRequests.Should().BeTrue();
        result.Value.Thumbnails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonStreamableFile_ShouldReturnNonStreamable()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
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
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        thumbnailRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(new List<FileThumbnail>());

        var query = new GetMediaStreamInfoQuery(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.IsStreamable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithThumbnails_ShouldIncludeThumbnailDtos()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
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
            UserId.New(), DateTimeOffset.UtcNow);
        fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var thumbnails = new List<FileThumbnail>
        {
            FileThumbnail.Create(file.Id, 200, 200, StorageKey.From("thumbs/200x200.webp"), DateTimeOffset.UtcNow),
            FileThumbnail.Create(file.Id, 800, 800, StorageKey.From("thumbs/800x800.webp"), DateTimeOffset.UtcNow)
        };
        thumbnailRepo.GetByFileIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(thumbnails);

        var query = new GetMediaStreamInfoQuery(file.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Thumbnails.Should().HaveCount(2);
        result.Value.Thumbnails[0].Width.Should().Be(200);
        result.Value.Thumbnails[1].Width.Should().Be(800);
    }

    [Fact]
    public async Task Handle_FileNotFound_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

        fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var query = new GetMediaStreamInfoQuery(FileId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*File not found*");
    }

    [Fact]
    public async Task Handle_WrongFamily_ShouldThrow()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var thumbnailRepo = Substitute.For<IFileThumbnailRepository>();
        var handler = new GetMediaStreamInfoQueryHandler(fileRepo, thumbnailRepo);

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

        var query = new GetMediaStreamInfoQuery(file.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Match("*File not found in this family*");
    }
}
