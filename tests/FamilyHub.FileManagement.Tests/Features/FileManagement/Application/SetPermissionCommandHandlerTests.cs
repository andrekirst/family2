using FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SetPermissionCommandHandlerTests
{
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly IFilePermissionRepository _permRepo = Substitute.For<IFilePermissionRepository>();
    private readonly SetPermissionCommandHandler _handler;

    public SetPermissionCommandHandlerTests()
    {
        _handler = new SetPermissionCommandHandler(_permRepo, _fileRepo, _folderRepo, TimeProvider.System);
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
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldCreateFilePermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();
        var grantedBy = UserId.New();

        var file = CreateTestFile(familyId);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.GetByMemberAndResourceAsync(memberId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns((FilePermission?)null);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            memberId,
            FilePermissionLevel.View)
        {
            FamilyId = familyId,
            UserId = grantedBy
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _permRepo.Received(1).AddAsync(
            Arg.Is<FilePermission>(p => p.MemberId == memberId && p.PermissionLevel == FilePermissionLevel.View),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateFolderPermission()
    {
        var familyId = FamilyId.New();
        var folder = Folder.Create(FileName.From("docs"), null, "/", familyId, UserId.New(), DateTimeOffset.UtcNow);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _permRepo.GetByMemberAndResourceAsync(UserId.New(), PermissionResourceType.Folder, folder.Id.Value, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((FilePermission?)null);

        var command = new SetPermissionCommand(
            PermissionResourceType.Folder,
            folder.Id.Value,
            UserId.New(),
            FilePermissionLevel.Edit)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _permRepo.Received(1).AddAsync(Arg.Any<FilePermission>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingPermission()
    {
        var familyId = FamilyId.New();
        var memberId = UserId.New();

        var file = CreateTestFile(familyId);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var existing = FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, memberId,
            FilePermissionLevel.View, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(memberId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            memberId,
            FilePermissionLevel.Manage)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        existing.PermissionLevel.Should().Be(FilePermissionLevel.Manage);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        _fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFolderNotFound()
    {
        _folderRepo.GetByIdAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Folder?)null);

        var command = new SetPermissionCommand(
            PermissionResourceType.Folder,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FolderNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var file = CreateTestFile(FamilyId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new SetPermissionCommand(
            PermissionResourceType.File,
            file.Id.Value,
            UserId.New(),
            FilePermissionLevel.View)
        {
            FamilyId = FamilyId.New(),
            UserId =
            UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
