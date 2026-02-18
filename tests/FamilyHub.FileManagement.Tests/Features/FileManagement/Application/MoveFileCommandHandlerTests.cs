using FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class MoveFileCommandHandlerTests
{
    private static StoredFile CreateTestFile(FamilyId familyId, FolderId folderId)
    {
        return StoredFile.Create(
            FileName.From("document.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folderId,
            familyId,
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldMoveFileToTargetFolder()
    {
        var familyId = FamilyId.New();
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();

        var sourceFolder = Folder.CreateRoot(familyId, UserId.New());
        var targetFolder = Folder.Create(
            FileName.From("Target"),
            sourceFolder.Id,
            $"/{sourceFolder.Id.Value}/",
            familyId,
            UserId.New());
        folderRepo.Folders.Add(sourceFolder);
        folderRepo.Folders.Add(targetFolder);

        var file = CreateTestFile(familyId, sourceFolder.Id);
        fileRepo.Files.Add(file);

        var handler = new MoveFileCommandHandler(fileRepo, folderRepo);

        var command = new MoveFileCommand(
            file.Id,
            targetFolder.Id,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.FileId.Should().Be(file.Id);
        file.FolderId.Should().Be(targetFolder.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();
        var handler = new MoveFileCommandHandler(fileRepo, folderRepo);

        var command = new MoveFileCommand(
            FileId.New(),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetFolderNotFound()
    {
        var familyId = FamilyId.New();
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();

        var sourceFolder = Folder.CreateRoot(familyId, UserId.New());
        folderRepo.Folders.Add(sourceFolder);

        var file = CreateTestFile(familyId, sourceFolder.Id);
        fileRepo.Files.Add(file);

        var handler = new MoveFileCommandHandler(fileRepo, folderRepo);

        var command = new MoveFileCommand(
            file.Id,
            FolderId.New(), // non-existent target
            familyId,
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();

        var sourceFolder = Folder.CreateRoot(familyId, UserId.New());
        var targetFolder = Folder.CreateRoot(otherFamilyId, UserId.New());
        folderRepo.Folders.Add(sourceFolder);
        folderRepo.Folders.Add(targetFolder);

        var file = CreateTestFile(familyId, sourceFolder.Id);
        fileRepo.Files.Add(file);

        var handler = new MoveFileCommandHandler(fileRepo, folderRepo);

        var command = new MoveFileCommand(
            file.Id,
            targetFolder.Id,
            familyId,
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
