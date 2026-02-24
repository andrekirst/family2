using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFilesByTag;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFilesByTagQueryHandlerTests
{
    private static (GetFilesByTagQueryHandler handler, FakeFileTagRepository fileTagRepo, FakeStoredFileRepository fileRepo) CreateHandler()
    {
        var fileTagRepo = new FakeFileTagRepository();
        var fileRepo = new FakeStoredFileRepository();
        var handler = new GetFilesByTagQueryHandler(fileTagRepo, fileRepo);
        return (handler, fileTagRepo, fileRepo);
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
        var (handler, fileTagRepo, fileRepo) = CreateHandler();
        var tagId = TagId.New();

        var file1 = CreateTestFile(familyId, "file1.jpg");
        var file2 = CreateTestFile(familyId, "file2.jpg");
        fileRepo.Files.Add(file1);
        fileRepo.Files.Add(file2);

        fileTagRepo.FileTags.Add(FileTag.Create(file1.Id, tagId));
        fileTagRepo.FileTags.Add(FileTag.Create(file2.Id, tagId));

        var query = new GetFilesByTagQuery([tagId], familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFilesWithAllTagsUsingAndLogic()
    {
        var familyId = FamilyId.New();
        var (handler, fileTagRepo, fileRepo) = CreateHandler();
        var tag1 = TagId.New();
        var tag2 = TagId.New();

        var file1 = CreateTestFile(familyId, "both-tags.jpg");
        var file2 = CreateTestFile(familyId, "only-tag1.jpg");
        fileRepo.Files.Add(file1);
        fileRepo.Files.Add(file2);

        // file1 has both tags, file2 has only tag1
        fileTagRepo.FileTags.Add(FileTag.Create(file1.Id, tag1));
        fileTagRepo.FileTags.Add(FileTag.Create(file1.Id, tag2));
        fileTagRepo.FileTags.Add(FileTag.Create(file2.Id, tag1));

        var query = new GetFilesByTagQuery([tag1, tag2], familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("both-tags.jpg");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoTagIds()
    {
        var (handler, _, _) = CreateHandler();

        var query = new GetFilesByTagQuery([], FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldFilterByFamily()
    {
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var (handler, fileTagRepo, fileRepo) = CreateHandler();
        var tagId = TagId.New();

        var myFile = CreateTestFile(familyId, "my-file.jpg");
        var otherFile = CreateTestFile(otherFamilyId, "other-file.jpg");
        fileRepo.Files.Add(myFile);
        fileRepo.Files.Add(otherFile);

        fileTagRepo.FileTags.Add(FileTag.Create(myFile.Id, tagId));
        fileTagRepo.FileTags.Add(FileTag.Create(otherFile.Id, tagId));

        var query = new GetFilesByTagQuery([tagId], familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("my-file.jpg");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoMatchingFiles()
    {
        var (handler, _, _) = CreateHandler();

        var query = new GetFilesByTagQuery([TagId.New()], FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
