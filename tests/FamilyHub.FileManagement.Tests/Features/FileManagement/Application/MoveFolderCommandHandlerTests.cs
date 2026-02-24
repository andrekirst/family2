using FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class MoveFolderCommandHandlerTests
{
    private static (MoveFolderCommandHandler handler, FakeFolderRepository folderRepo) CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var handler = new MoveFolderCommandHandler(folderRepo);
        return (handler, folderRepo);
    }

    [Fact]
    public async Task Handle_ShouldMoveFolderAndUpdatePaths()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        // Create: root → A → B (child of A)
        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folderA);

        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId);
        folderRepo.Folders.Add(folderB);

        var folderC = Folder.Create(FileName.From("C"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folderC);

        // Move A under C
        var command = new MoveFolderCommand(folderA.Id, folderC.Id, familyId, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.FolderId.Should().Be(folderA.Id);
        folderA.ParentFolderId.Should().Be(folderC.Id);

        // A's new path should be under C
        folderA.MaterializedPath.Should().Be($"/{root.Id.Value}/{folderC.Id.Value}/");

        // B (descendant of A) should have updated path too
        folderB.MaterializedPath.Should().Be($"/{root.Id.Value}/{folderC.Id.Value}/{folderA.Id.Value}/");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMovingFolderIntoItself()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var command = new MoveFolderCommand(folder.Id, folder.Id, familyId, userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMovingFolderIntoDescendant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folderA);

        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId);
        folderRepo.Folders.Add(folderB);

        // Try to move A into B (its child) — should fail
        var command = new MoveFolderCommand(folderA.Id, folderB.Id, familyId, userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new MoveFolderCommand(FolderId.New(), FolderId.New(), FamilyId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetFolderNotFound()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var command = new MoveFolderCommand(folder.Id, FolderId.New(), familyId, userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetBelongsToDifferentFamily()
    {
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root1 = Folder.CreateRoot(familyId1, userId);
        folderRepo.Folders.Add(root1);

        var folder = Folder.Create(FileName.From("A"), root1.Id, $"/{root1.Id.Value}/", familyId1, userId);
        folderRepo.Folders.Add(folder);

        var root2 = Folder.CreateRoot(familyId2, userId);
        folderRepo.Folders.Add(root2);

        var command = new MoveFolderCommand(folder.Id, root2.Id, familyId1, userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
