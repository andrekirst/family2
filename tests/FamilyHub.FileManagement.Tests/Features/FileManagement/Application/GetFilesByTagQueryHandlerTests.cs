using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFilesByTag;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFilesByTagQueryHandlerTests
{
    private readonly IFileTagRepository _fileTagRepo = Substitute.For<IFileTagRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly GetFilesByTagQueryHandler _handler;

    public GetFilesByTagQueryHandlerTests()
    {
        _handler = new GetFilesByTagQueryHandler(_fileTagRepo, _fileRepo);
    }

    private static StoredFile CreateTestFile(FamilyId familyId, string name = "photo.jpg")
    {
        return StoredFile.Create(
            FileName.From(name),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldReturnFilesWithSingleTag()
    {
        var familyId = FamilyId.New();
        var tagId = TagId.New();

        var file1 = CreateTestFile(familyId, "file1.jpg");
        var file2 = CreateTestFile(familyId, "file2.jpg");

        _fileTagRepo.GetFileIdsByTagIdsAsync(
            Arg.Is<List<TagId>>(ids => ids.Count == 1 && ids[0] == tagId),
            Arg.Any<CancellationToken>())
            .Returns(new List<FileId> { file1.Id, file2.Id });
        _fileRepo.GetByIdsAsync(
            Arg.Is<List<FileId>>(ids => ids.Count == 2),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file1, file2 });

        var query = new GetFilesByTagQuery([tagId])
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilesWithAllTagsUsingAndLogic()
    {
        var familyId = FamilyId.New();
        var tag1 = TagId.New();
        var tag2 = TagId.New();

        var file1 = CreateTestFile(familyId, "both-tags.jpg");

        _fileTagRepo.GetFileIdsByTagIdsAsync(
            Arg.Is<List<TagId>>(ids => ids.Count == 2),
            Arg.Any<CancellationToken>())
            .Returns(new List<FileId> { file1.Id });
        _fileRepo.GetByIdsAsync(
            Arg.Any<List<FileId>>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file1 });

        var query = new GetFilesByTagQuery([tag1, tag2])
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("both-tags.jpg");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoTagIds()
    {
        var query = new GetFilesByTagQuery([])
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var familyId = FamilyId.New();
        var tagId = TagId.New();

        var myFile = CreateTestFile(familyId, "my-file.jpg");

        _fileTagRepo.GetFileIdsByTagIdsAsync(
            Arg.Any<List<TagId>>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<FileId> { myFile.Id });
        _fileRepo.GetByIdsAsync(
            Arg.Any<List<FileId>>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { myFile });

        var query = new GetFilesByTagQuery([tagId])
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("my-file.jpg");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoMatchingFiles()
    {
        _fileTagRepo.GetFileIdsByTagIdsAsync(
            Arg.Any<List<TagId>>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<FileId>());

        var query = new GetFilesByTagQuery([TagId.New()])
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
