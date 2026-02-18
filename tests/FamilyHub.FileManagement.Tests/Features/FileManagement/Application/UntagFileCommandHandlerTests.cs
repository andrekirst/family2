using FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UntagFileCommandHandlerTests
{
    private static (UntagFileCommandHandler handler, FakeStoredFileRepository fileRepo, FakeFileTagRepository fileTagRepo) CreateHandler()
    {
        var fileRepo = new FakeStoredFileRepository();
        var fileTagRepo = new FakeFileTagRepository();
        var handler = new UntagFileCommandHandler(fileRepo, fileTagRepo);
        return (handler, fileRepo, fileTagRepo);
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
    public async Task Handle_ShouldRemoveTag()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, fileTagRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var tagId = TagId.New();
        fileTagRepo.FileTags.Add(FileTag.Create(file.Id, tagId));

        var command = new UntagFileCommand(file.Id, tagId, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        fileTagRepo.FileTags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotentWhenNotTagged()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, _) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new UntagFileCommand(file.Id, TagId.New(), familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var (handler, _, _) = CreateHandler();

        var command = new UntagFileCommand(FileId.New(), TagId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var (handler, fileRepo, _) = CreateHandler();

        var file = CreateTestFile(FamilyId.New());
        fileRepo.Files.Add(file);

        var command = new UntagFileCommand(file.Id, TagId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
