using FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class MoveFolderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMoveFolderAndUpdatePaths()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);
        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId, DateTimeOffset.UtcNow);
        var folderC = Folder.Create(FileName.From("C"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folderA.Id, Arg.Any<CancellationToken>()).Returns(folderA);
        folderRepo.GetByIdAsync(folderC.Id, Arg.Any<CancellationToken>()).Returns(folderC);
        folderRepo.GetDescendantsAsync(Arg.Any<string>(), familyId, Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { folderB });

        var command = new MoveFolderCommand(folderA.Id, folderC.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.FolderId.Should().Be(folderA.Id);
        folderA.ParentFolderId.Should().Be(folderC.Id);
        folderA.MaterializedPath.Should().Be($"/{root.Id.Value}/{folderC.Id.Value}/");
        folderB.MaterializedPath.Should().Be($"/{root.Id.Value}/{folderC.Id.Value}/{folderA.Id.Value}/");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMovingFolderIntoItself()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new MoveFolderCommand(folder.Id, folder.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMovingFolderIntoDescendant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);
        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folderA.Id, Arg.Any<CancellationToken>()).Returns(folderA);
        folderRepo.GetByIdAsync(folderB.Id, Arg.Any<CancellationToken>()).Returns(folderB);
        folderRepo.GetDescendantsAsync(Arg.Any<string>(), familyId, Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { folderB });

        var command = new MoveFolderCommand(folderA.Id, folderB.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var command = new MoveFolderCommand(FolderId.New(), FolderId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetFolderNotFound()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var targetId = FolderId.New();
        folderRepo.GetByIdAsync(targetId, Arg.Any<CancellationToken>()).Returns((Folder?)null);

        var command = new MoveFolderCommand(folder.Id, targetId)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTargetBelongsToDifferentFamily()
    {
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        var userId = UserId.New();
        var folderRepo = Substitute.For<IFolderRepository>();
        var handler = new MoveFolderCommandHandler(folderRepo, TimeProvider.System);

        var root1 = Folder.CreateRoot(familyId1, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(FileName.From("A"), root1.Id, $"/{root1.Id.Value}/", familyId1, userId, DateTimeOffset.UtcNow);
        var root2 = Folder.CreateRoot(familyId2, userId, DateTimeOffset.UtcNow);

        folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        folderRepo.GetByIdAsync(root2.Id, Arg.Any<CancellationToken>()).Returns(root2);

        var command = new MoveFolderCommand(folder.Id, root2.Id)
        {
            FamilyId = familyId1,
            UserId = userId
        };
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
