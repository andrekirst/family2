using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Services;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

/// <summary>
/// A list-based FamilyMemberRepository fake that supports multiple members
/// for authorization service testing.
/// </summary>
internal class ListFakeFamilyMemberRepository : IFamilyMemberRepository
{
    public List<FamilyMember> Members { get; } = [];

    public Task<FamilyMember?> GetByUserAndFamilyAsync(UserId userId, FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Members.FirstOrDefault(m => m.UserId == userId && m.FamilyId == familyId));

    public Task<List<FamilyMember>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Members.Where(m => m.FamilyId == familyId).ToList());

    public Task AddAsync(FamilyMember member, CancellationToken ct = default)
    {
        Members.Add(member);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(1);
}

public class FileManagementAuthorizationServiceTests
{
    private static (FileManagementAuthorizationService svc, FakeFilePermissionRepository permRepo, FakeStoredFileRepository fileRepo, FakeFolderRepository folderRepo, ListFakeFamilyMemberRepository memberRepo) CreateService()
    {
        var permRepo = new FakeFilePermissionRepository();
        var fileRepo = new FakeStoredFileRepository();
        var folderRepo = new FakeFolderRepository();
        var memberRepo = new ListFakeFamilyMemberRepository();
        var svc = new FileManagementAuthorizationService(permRepo, fileRepo, folderRepo, memberRepo);
        return (svc, permRepo, fileRepo, folderRepo, memberRepo);
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
            uploadedBy);
    }

    [Fact]
    public async Task HasFilePermission_UnrestrictedFile_ShouldAllowAccess()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (svc, _, fileRepo, _, _) = CreateService();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        fileRepo.Files.Add(file);

        var result = await svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_RestrictedFile_ShouldDenyWithoutGrant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (svc, permRepo, fileRepo, _, _) = CreateService();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        fileRepo.Files.Add(file);

        // Restrict file to another user
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));

        var result = await svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasFilePermission_RestrictedFile_ShouldAllowWithGrant()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (svc, permRepo, fileRepo, _, _) = CreateService();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        fileRepo.Files.Add(file);

        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, userId,
            FilePermissionLevel.Edit, familyId, UserId.New()));

        var result = await svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_InsufficientLevel_ShouldDeny()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (svc, permRepo, fileRepo, _, _) = CreateService();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        fileRepo.Files.Add(file);

        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, userId,
            FilePermissionLevel.View, familyId, UserId.New()));

        var result = await svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.Edit, familyId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasFilePermission_OwnerBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var ownerId = UserId.New();
        var (svc, permRepo, fileRepo, _, _) = CreateService();

        var file = CreateTestFile(familyId, ownerId, FolderId.New());
        fileRepo.Files.Add(file);

        // Restrict file to another user
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));

        var result = await svc.HasFilePermissionAsync(
            ownerId, file.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_FamilyAdminBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var adminId = UserId.New();
        var (svc, permRepo, fileRepo, _, memberRepo) = CreateService();

        var file = CreateTestFile(familyId, UserId.New(), FolderId.New());
        fileRepo.Files.Add(file);

        // Make user a family admin
        memberRepo.Members.Add(FamilyMember.Create(familyId, adminId, FamilyRole.From("Admin")));

        // Restrict file
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, file.Id.Value, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));

        var result = await svc.HasFilePermissionAsync(
            adminId, file.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFilePermission_FolderInheritance_ShouldInheritFromParent()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (svc, permRepo, fileRepo, folderRepo, _) = CreateService();

        // Create parent folder with restrictions
        var parentFolder = Folder.Create(FileName.From("restricted"), null, "/", familyId, UserId.New());
        folderRepo.Folders.Add(parentFolder);

        // Grant permission on parent folder
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.Folder, parentFolder.Id.Value, userId,
            FilePermissionLevel.Edit, familyId, UserId.New()));

        // Create file in the restricted folder
        var file = CreateTestFile(familyId, UserId.New(), parentFolder.Id);
        fileRepo.Files.Add(file);

        var result = await svc.HasFilePermissionAsync(
            userId, file.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFolderPermission_UnrestrictedFolder_ShouldAllowAccess()
    {
        var familyId = FamilyId.New();
        var (svc, _, _, folderRepo, _) = CreateService();

        var folder = Folder.Create(FileName.From("public"), null, "/", familyId, UserId.New());
        folderRepo.Folders.Add(folder);

        var result = await svc.HasFolderPermissionAsync(
            UserId.New(), folder.Id, FilePermissionLevel.View, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasFolderPermission_CreatorBypass_ShouldAlwaysAllow()
    {
        var familyId = FamilyId.New();
        var creatorId = UserId.New();
        var (svc, permRepo, _, folderRepo, _) = CreateService();

        var folder = Folder.Create(FileName.From("private"), null, "/", familyId, creatorId);
        folderRepo.Folders.Add(folder);

        // Restrict folder to another user
        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.Folder, folder.Id.Value, UserId.New(),
            FilePermissionLevel.View, familyId, UserId.New()));

        var result = await svc.HasFolderPermissionAsync(
            creatorId, folder.Id, FilePermissionLevel.Manage, familyId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsResourceRestricted_ShouldReturnTrueWhenPermissionsExist()
    {
        var (svc, permRepo, _, _, _) = CreateService();
        var resourceId = Guid.NewGuid();

        permRepo.Permissions.Add(FilePermission.Create(
            PermissionResourceType.File, resourceId, UserId.New(),
            FilePermissionLevel.View, FamilyId.New(), UserId.New()));

        var result = await svc.IsResourceRestrictedAsync(
            PermissionResourceType.File, resourceId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsResourceRestricted_ShouldReturnFalseWhenNoPermissions()
    {
        var (svc, _, _, _, _) = CreateService();

        var result = await svc.IsResourceRestrictedAsync(
            PermissionResourceType.File, Guid.NewGuid());

        result.Should().BeFalse();
    }
}
