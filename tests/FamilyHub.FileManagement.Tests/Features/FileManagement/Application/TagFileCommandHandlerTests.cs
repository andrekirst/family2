using FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class TagFileCommandHandlerTests
{
    private static (TagFileCommandHandler handler, FakeStoredFileRepository fileRepo, FakeTagRepository tagRepo, FakeFileTagRepository fileTagRepo) CreateHandler()
    {
        var fileRepo = new FakeStoredFileRepository();
        var tagRepo = new FakeTagRepository();
        var fileTagRepo = new FakeFileTagRepository();
        var handler = new TagFileCommandHandler(fileRepo, tagRepo, fileTagRepo);
        return (handler, fileRepo, tagRepo, fileTagRepo);
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
    public async Task Handle_ShouldTagFile()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, tagRepo, fileTagRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new TagFileCommand(file.Id, tag.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        fileTagRepo.FileTags.Should().HaveCount(1);
        fileTagRepo.FileTags.First().FileId.Should().Be(file.Id);
        fileTagRepo.FileTags.First().TagId.Should().Be(tag.Id);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, tagRepo, fileTagRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        // Pre-add the file-tag association
        fileTagRepo.FileTags.Add(FileTag.Create(file.Id, tag.Id));

        var command = new TagFileCommand(file.Id, tag.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        fileTagRepo.FileTags.Should().HaveCount(1); // No duplicate
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var (handler, _, tagRepo, _) = CreateHandler();
        var familyId = FamilyId.New();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new TagFileCommand(FileId.New(), tag.Id, familyId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        var familyId = FamilyId.New();
        var (handler, fileRepo, _, _) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new TagFileCommand(file.Id, TagId.New(), familyId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var (handler, fileRepo, tagRepo, _) = CreateHandler();

        var file = CreateTestFile(FamilyId.New());
        fileRepo.Files.Add(file);

        var differentFamily = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), differentFamily, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new TagFileCommand(file.Id, tag.Id, differentFamily);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
