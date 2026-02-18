using FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SetPermissionCommandHandlerTests
{
    private static (SetPermissionCommandHandler handler, FakeStoredFileRepository fileRepo, FakeFolderRepository folderRepo, FakeFilePermissionRepository permRepo) CreateHandler()
    {
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();
        var permRepo = new FakeFilePermissionRepository();
        var handler = new SetPermissionCommandHandler(permRepo, fileRepo, folderRepo);
        return (handler, fileRepo, folderRepo, permRepo);
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
    public async Task Handle_ShouldCreateFilePermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();
        var grantedBy = UserId.New();
        var (handler, fileRepo, _, permRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            memberId,
            FilePermissionLevel.View,
            familyId,
            grantedBy);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        permRepo.Permissions.Should().HaveCount(1);
        permRepo.Permissions.First().MemberId.Should().Be(memberId);
        permRepo.Permissions.First().PermissionLevel.Should().Be(FilePermissionLevel.View);
    }

    [Fact]
    public async Task Handle_ShouldCreateFolderPermission()
    {
        var familyId = FamilyId.New();
        var (handler, _, folderRepo, permRepo) = CreateHandler();

        var folder = Folder.Create(FileName.From("docs"), null, "/", familyId, UserId.New());
        folderRepo.Folders.Add(folder);

        var command = new SetPermissionCommand(
            PermissionResourceType.Folder,
            folder.Id.Value,
            UserId.New(),
            FilePermissionLevel.Edit,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        permRepo.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingPermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();
        var (handler, fileRepo, _, permRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        // Pre-add permission
        var existing = FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, memberId,
            FilePermissionLevel.View, familyId, UserId.New());
        permRepo.Permissions.Add(existing);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            memberId,
            FilePermissionLevel.Manage,
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        permRepo.Permissions.Should().HaveCount(1);
        permRepo.Permissions.First().PermissionLevel.Should().Be(FilePermissionLevel.Manage);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var (handler, _, _, _) = CreateHandler();

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        var (handler, _, _, _) = CreateHandler();

        var command = new SetPermissionCommand(
            PermissionResourceType.Folder,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(),
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var (handler, fileRepo, _, _) = CreateHandler();

        var file = CreateTestFile(FamilyId.New());
        fileRepo.Files.Add(file);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(), // Different family
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
