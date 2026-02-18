using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateFolderCommandHandlerTests
{
    private static (CreateFolderCommandHandler handler, FakeFolderRepository folderRepo) CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var handler = new CreateFolderCommandHandler(folderRepo);
        return (handler, folderRepo);
    }

    [Fact]
    public async Task Handle_WithParent_ShouldCreateFolderWithCorrectMaterializedPath()
    {
        var familyId = FamilyId.New();
        var (handler, folderRepo) = CreateHandler();

        var parentFolder = Folder.CreateRoot(familyId, UserId.New());
        folderRepo.Folders.Add(parentFolder);

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            parentFolder.Id,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FolderId.Value.Should().NotBe(Guid.Empty);

        var createdFolder = folderRepo.Folders.First(f => f.Id == result.FolderId);
        createdFolder.MaterializedPath.Should().Be($"/{parentFolder.Id.Value}/");
        createdFolder.ParentFolderId.Should().Be(parentFolder.Id);
    }

    [Fact]
    public async Task Handle_WithoutParent_ShouldAutoCreateRootFolder()
    {
        var familyId = FamilyId.New();
        var (handler, folderRepo) = CreateHandler();

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            null,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        // Should have created 2 folders: root + new folder
        folderRepo.Folders.Should().HaveCount(2);

        var rootFolder = folderRepo.Folders.First(f => f.ParentFolderId == null);
        rootFolder.Name.Value.Should().Be("Root");
        rootFolder.MaterializedPath.Should().Be("/");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenParentFolderNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new CreateFolderCommand(
            FileName.From("SubFolder"),
            FolderId.New(),
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenParentBelongsToDifferentFamily()
    {
        var (handler, folderRepo) = CreateHandler();
        var parentFolder = Folder.CreateRoot(FamilyId.New(), UserId.New());
        folderRepo.Folders.Add(parentFolder);

        var command = new CreateFolderCommand(
            FileName.From("SubFolder"),
            parentFolder.Id,
            FamilyId.New(), // different family
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_WithExistingRoot_ShouldUseExistingRoot()
    {
        var familyId = FamilyId.New();
        var (handler, folderRepo) = CreateHandler();

        var existingRoot = Folder.CreateRoot(familyId, UserId.New());
        folderRepo.Folders.Add(existingRoot);

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            null,
            familyId,
            UserId.New());

        await handler.Handle(command, CancellationToken.None);

        // Should have 2 folders: existing root + new folder (no duplicate root)
        folderRepo.Folders.Should().HaveCount(2);
        folderRepo.Folders.Count(f => f.ParentFolderId == null).Should().Be(1);
    }
}
