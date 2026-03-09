using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateFolderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithParent_ShouldCreateFolderWithCorrectMaterializedPath()
    {
        var familyId = FamilyId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new CreateFolderCommandHandler(folderRepo);

        var parentFolder = Folder.CreateRoot(familyId, UserId.New());
        folderRepo.GetByIdAsync(parentFolder.Id, Arg.Any<CancellationToken>()).Returns(parentFolder);

        Folder? capturedFolder = null;
        folderRepo.AddAsync(Arg.Do<Folder>(f => capturedFolder = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            parentFolder.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FolderId.Value.Should().NotBe(Guid.Empty);
        capturedFolder.Should().NotBeNull();
        capturedFolder!.MaterializedPath.Should().Be($"/{parentFolder.Id.Value}/");
        capturedFolder.ParentFolderId.Should().Be(parentFolder.Id);
    }

    [Fact]
    public async Task Handle_WithoutParent_ShouldAutoCreateRootFolder()
    {
        var familyId = FamilyId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new CreateFolderCommandHandler(folderRepo);

        folderRepo.GetRootFolderAsync(familyId, Arg.Any<CancellationToken>())
            .Returns((Folder?)null);

        var addedFolders = new List<Folder>();
        folderRepo.AddAsync(Arg.Do<Folder>(f => addedFolders.Add(f)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // When AddAsync is called for root, make GetRootFolderAsync return it on subsequent calls
        folderRepo.When(r => r.AddAsync(Arg.Is<Folder>(f => f.ParentFolderId == null), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                var root = ci.Arg<Folder>();
                folderRepo.GetRootFolderAsync(familyId, Arg.Any<CancellationToken>()).Returns(root);
            });

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            null)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        addedFolders.Should().HaveCount(2);
        var rootFolder = addedFolders.First(f => f.ParentFolderId == null);
        rootFolder.Name.Value.Should().Be("Root");
        rootFolder.MaterializedPath.Should().Be("/");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenParentFolderNotFound()
    {
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new CreateFolderCommandHandler(folderRepo);

        folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var command = new CreateFolderCommand(
            FileName.From("SubFolder"),
            FolderId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenParentBelongsToDifferentFamily()
    {
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new CreateFolderCommandHandler(folderRepo);

        var parentFolder = Folder.CreateRoot(FamilyId.New(), UserId.New());
        folderRepo.GetByIdAsync(parentFolder.Id, Arg.Any<CancellationToken>()).Returns(parentFolder);

        var command = new CreateFolderCommand(
            FileName.From("SubFolder"),
            parentFolder.Id)
        {
            FamilyId = FamilyId.New(),
            UserId =
            UserId.New()
        };

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_WithExistingRoot_ShouldUseExistingRoot()
    {
        var familyId = FamilyId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new CreateFolderCommandHandler(folderRepo);

        var existingRoot = Folder.CreateRoot(familyId, UserId.New());
        folderRepo.GetRootFolderAsync(familyId, Arg.Any<CancellationToken>()).Returns(existingRoot);

        var addedFolders = new List<Folder>();
        folderRepo.AddAsync(Arg.Do<Folder>(f => addedFolders.Add(f)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new CreateFolderCommand(
            FileName.From("Documents"),
            null)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        await handler.Handle(command, CancellationToken.None);

        // Should have added 1 folder (new folder only, no duplicate root)
        addedFolders.Should().HaveCount(1);
        addedFolders[0].ParentFolderId.Should().Be(existingRoot.Id);
    }
}
