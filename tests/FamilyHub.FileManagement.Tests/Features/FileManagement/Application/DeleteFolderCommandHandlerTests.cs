using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteFolderCommandHandlerTests
{
    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IFileManagementStorageService _storageService = Substitute.For<IFileManagementStorageService>();
    private readonly DeleteFolderCommandHandler _handler;

    public DeleteFolderCommandHandlerTests()
    {
        _handler = new DeleteFolderCommandHandler(_folderRepo, _fileRepo, _storageService, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFolderAndFiles()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(FileName.From("Documents"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);

        var file = StoredFile.Create(
            FileName.From("test.pdf"), MimeType.From("application/pdf"),
            FileSize.From(1024), StorageKey.From("key-1"),
            Checksum.From("a".PadRight(64, 'a')), folder.Id, familyId, userId, DateTimeOffset.UtcNow);

        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _folderRepo.GetDescendantsAsync(Arg.Any<string>(), familyId, Arg.Any<CancellationToken>())
            .Returns(new List<Folder>());
        _fileRepo.GetByFolderIdsAsync(Arg.Any<List<FolderId>>(), Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var command = new DeleteFolderCommand(folder.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _folderRepo.Received(1).RemoveAsync(folder, Arg.Any<CancellationToken>());
        await _fileRepo.Received(1).RemoveRangeAsync(
            Arg.Is<List<StoredFile>>(files => files.Contains(file)),
            Arg.Any<CancellationToken>());
        await _storageService.Received(1).DeleteFileAsync(familyId, "key-1", 1024, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRecursivelyDeleteDescendants()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folderA = Folder.Create(FileName.From("A"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);
        var folderB = Folder.Create(
            FileName.From("B"), folderA.Id,
            $"/{root.Id.Value}/{folderA.Id.Value}/",
            familyId, userId, DateTimeOffset.UtcNow);

        var file = StoredFile.Create(
            FileName.From("deep.txt"), MimeType.From("text/plain"),
            FileSize.From(512), StorageKey.From("key-deep"),
            Checksum.From("b".PadRight(64, 'b')), folderB.Id, familyId, userId, DateTimeOffset.UtcNow);

        _folderRepo.GetByIdAsync(folderA.Id, Arg.Any<CancellationToken>()).Returns(folderA);
        _folderRepo.GetDescendantsAsync(Arg.Any<string>(), familyId, Arg.Any<CancellationToken>())
            .Returns(new List<Folder> { folderB });
        _fileRepo.GetByFolderIdsAsync(
            Arg.Is<List<FolderId>>(ids => ids.Contains(folderB.Id)),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var command = new DeleteFolderCommand(folderA.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _folderRepo.Received(1).RemoveAsync(folderA, Arg.Any<CancellationToken>());
        await _storageService.Received(1).DeleteFileAsync(familyId, "key-deep", 512, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDeletingRootFolder()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        _folderRepo.GetByIdAsync(root.Id, Arg.Any<CancellationToken>()).Returns(root);

        var command = new DeleteFolderCommand(root.Id)
        {
            FamilyId = familyId,
            UserId = userId
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderBelongsToDifferentFamily()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId, DateTimeOffset.UtcNow);
        var folder = Folder.Create(FileName.From("Docs"), root.Id, $"/{root.Id.Value}/", familyId, userId, DateTimeOffset.UtcNow);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

        var command = new DeleteFolderCommand(folder.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = userId
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
