using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameFolderCommandHandlerTests
{
    private static (RenameFolderCommandHandler handler, FakeFolderRepository folderRepo) CreateHandler()
    {
        var folderRepo = new FakeFolderRepository();
        var handler = new RenameFolderCommandHandler(folderRepo);
        return (handler, folderRepo);
    }

    [Fact]
    public async Task Handle_ShouldRenameFolder()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, folderRepo) = CreateHandler();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("OldName"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var command = new RenameFolderCommand(folder.Id, FileName.From("NewName"), familyId, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.FolderId.Should().Be(folder.Id);
        folder.Name.Value.Should().Be("NewName");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new RenameFolderCommand(FolderId.New(), FileName.From("NewName"), FamilyId.New(), UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var (handler, folderRepo) = CreateHandler();
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId);
        folderRepo.Folders.Add(root);

        var folder = Folder.Create(FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        folderRepo.Folders.Add(folder);

        var command = new RenameFolderCommand(folder.Id, FileName.From("NewName"), FamilyId.New(), userId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
