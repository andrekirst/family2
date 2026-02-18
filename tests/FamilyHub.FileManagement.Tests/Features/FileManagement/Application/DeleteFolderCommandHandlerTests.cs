using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteFolderCommandHandlerTests
{
    private static (DeleteFolderCommandHandler handler, FakeFolderRepository folderRepo, FakeStoredFileRepository fileRepo, FakeFileManagementStorageService storageService)
        CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var fileRepo = new FakeStoredFileRepository();
        var storageService = new FakeFileManagementStorageService();
        var handler = new DeleteFolderCommandHandler(folderRepo, fileRepo, storageService);
        return (handler, folderRepo, fileRepo, storageService);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFolderAndFiles()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo, fileRepo, storageService) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var file = StoredFile.Create(
            FileName.From("test.pdf"), MimeType.From("application/pdf"),
            FileSize.From(1024), StorageKey.From("key-1"),
            Checksum.From("a".PadRight(64, 'a')), folder.Id, familyId, userId);
        fileRepo.Files.Add(file);

        var command = new DeleteFolderCommand(folder.Id, familyId, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        folderRepo.Folders.Should().NotContain(folder);
        fileRepo.Files.Should().BeEmpty();
        storageService.DeletedStorageKeys.Should().Contain("key-1");
    }

    [Fact]
    public async Task Handle_ShouldRecursivelyDeleteDescendants()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo, fileRepo, storageService) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folderA);

        // B is child of A
        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId);
        folderRepo.Folders.Add(folderB);

        // File in B
        var file = StoredFile.Create(
            FileName.From("deep.txt"), MimeType.From("text/plain"),
            FileSize.From(512), StorageKey.From("key-deep"),
            Checksum.From("b".PadRight(64, 'b')), folderB.Id, familyId, userId);
        fileRepo.Files.Add(file);

        // Delete A — should cascade to B and its file
        var command = new DeleteFolderCommand(folderA.Id, familyId, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        folderRepo.Folders.Should().HaveCount(1); // Only root remains
        folderRepo.Folders.Single().Id.Should().Be(root.Id);
        fileRepo.Files.Should().BeEmpty();
        storageService.DeletedStorageKeys.Should().Contain("key-deep");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDeletingRootFolder()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo, _, _) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var command = new DeleteFolderCommand(root.Id, familyId, userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _, _, _) = CreateHandler();

        var command = new DeleteFolderCommand(FolderId.New(), FamilyId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo, _, _) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("Docs"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var command = new DeleteFolderCommand(folder.Id, FamilyId.New(), userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
