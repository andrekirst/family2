using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameFolderCommandHandlerTests
{
    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly RenameFolderCommandHandler _handler;

    public RenameFolderCommandHandlerTests()
    {
        _handler = new RenameFolderCommandHandler(_folderRepo);
    }

    [Fact]
    public async Task Handle_ShouldRenameFolder()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var root = Folder.CreateRoot(familyId, userId);
        var folder = Folder.Create(FileName.From("OldName"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new RenameFolderCommand(folder.Id, FileName.From("NewName"))
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.FolderId.Should().Be(folder.Id);
        folder.Name.Value.Should().Be("NewName");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        _folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var command = new RenameFolderCommand(FolderId.New(), FileName.From("NewName"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var root = Folder.CreateRoot(familyId, userId);
        var folder = Folder.Create(FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new RenameFolderCommand(folder.Id, FileName.From("NewName"))
        {
            FamilyId = FamilyId.New(),
            UserId = userId
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
