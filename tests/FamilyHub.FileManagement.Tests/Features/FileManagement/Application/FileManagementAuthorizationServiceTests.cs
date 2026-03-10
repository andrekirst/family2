using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class FileManagementAuthorizationServiceTests
{
    private readonly IFilePermissionRepository _permRepo = Substitute.For<IFilePermissionRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IFolderRepository _folderRepo = Substitute.For<IFolderRepository>();
    private readonly IFamilyMemberRepository _memberRepo = Substitute.For<IFamilyMemberRepository>();
    private readonly FileManagementAuthorizationService _svc;

    public FileManagementAuthorizationServiceTests()
    {
        _svc = new FileManagementAuthorizationService(_permRepo, _fileRepo, _folderRepo, _memberRepo);

        // Default: GetAncestorsAsync returns empty list to prevent NPE in CheckFolderInheritanceAsync
        _folderRepo.GetAncestorsAsync(FolderId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<Folder>());
    }

    private static StoredFile CreateTestFile(FamilyId familyId, UserId uploadedBy, FolderId folderId)
    {
        return StoredFile.Create(
            FileName.From("test.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folderId,
            familyId,
            uploadedBy, DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task HasFilePermission_UnrestrictedFile_ShouldAllowAccess()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_RestrictedFile_ShouldDenyWithoutGrant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);
        _permRepo.GetByMemberAndResourceAsync(userId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns((FilePermission?)null);
        _memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns((FamilyMember?)null);

        var result = await _svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasFilePermission_RestrictedFile_ShouldAllowWithGrant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);

        var grant = FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, userId,
            FilePermissionLevel.Edit, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(userId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(grant);

        var result = await _svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_InsufficientLevel_ShouldDeny()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);

        var grant = FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, userId,
            FilePermissionLevel.View, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(userId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(grant);
        _memberRepo.GetByUserAndFamilyAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns((FamilyMember?)null);

        var result = await _svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.Edit, familyId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasFilePermission_OwnerBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var ownerId = UserId.New();

        var file = CreateTestFile(familyId, ownerId, FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);
        _permRepo.GetByMemberAndResourceAsync(ownerId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns((FilePermission?)null);

        var result = await _svc.HasFilePermissionAsync(
            ownerId, file.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_FamilyAdminBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var adminId = UserId.New();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);
        _permRepo.GetByMemberAndResourceAsync(adminId, PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns((FilePermission?)null);
        _memberRepo.GetByUserAndFamilyAsync(adminId, familyId, Arg.Any<CancellationToken>())
            .Returns(FamilyMember.Create(familyId, adminId, FamilyRole.From("Admin"), DateTimeOffset.UtcNow));

        var result = await _svc.HasFilePermissionAsync(
            adminId, file.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_FolderInheritance_ShouldInheritFromParent()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var parentFolder = Folder.Create(FileName.From("restricted"), null, "/", familyId, UserId.New(), DateTimeOffset.UtcNow);
        var file = CreateTestFile(familyId, UserId.New(), parentFolder.Id);
        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _folderRepo.GetByIdAsync(parentFolder.Id, Arg.Any<CancellationToken>()).Returns(parentFolder);

        // File-level: restricted but no direct grant
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, file.Id.Value, Arg.Any<CancellationToken>())
            .Returns(false);
        // Folder-level: has permissions, grant to user
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.Folder, parentFolder.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);
        var folderGrant = FilePermission.Create(
            PermissionResourceType.Folder, parentFolder.Id.Value, userId,
            FilePermissionLevel.Edit, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _permRepo.GetByMemberAndResourceAsync(userId, PermissionResourceType.Folder, parentFolder.Id.Value, Arg.Any<CancellationToken>())
            .Returns(folderGrant);

        var result = await _svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFolderPermission_UnrestrictedFolder_ShouldAllowAccess()
    {
        var familyId = FamilyId.New();

        var folder = Folder.Create(FileName.From("public"), null, "/", familyId, UserId.New(), DateTimeOffset.UtcNow);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.Folder, folder.Id.Value, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _svc.HasFolderPermissionAsync(
            UserId.New(), folder.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFolderPermission_CreatorBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var creatorId = UserId.New();

        var folder = Folder.Create(FileName.From("private"), null, "/", familyId, creatorId, DateTimeOffset.UtcNow);
        _folderRepo.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.Folder, folder.Id.Value, Arg.Any<CancellationToken>())
            .Returns(true);
        _permRepo.GetByMemberAndResourceAsync(creatorId, PermissionResourceType.Folder, folder.Id.Value, Arg.Any<CancellationToken>())
            .Returns((FilePermission?)null);

        var result = await _svc.HasFolderPermissionAsync(
            creatorId, folder.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsResourceRestricted_ShouldReturnTrueWhenPermissionsExist()
    {
        var resourceId = Guid.NewGuid();
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, resourceId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _svc.IsResourceRestrictedAsync(
            PermissionResourceType.File, resourceId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsResourceRestricted_ShouldReturnFalseWhenNoPermissions()
    {
        _permRepo.HasAnyPermissionsAsync(PermissionResourceType.File, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _svc.IsResourceRestrictedAsync(
            PermissionResourceType.File, Guid.NewGuid());

        result.Should().BeFalse();
    }
}
