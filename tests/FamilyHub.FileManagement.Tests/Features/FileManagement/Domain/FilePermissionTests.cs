using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class FilePermissionTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var memberId = UserId.New();
        var familyId = FamilyId.New();
        var grantedBy = UserId.New();
        var resourceId = Guid.NewGuid();

        var permission = FilePermission.Create(
            PermissionResourceType.File,
            resourceId,
            memberId,
            FilePermissionLevel.Edit,
            familyId,
            grantedBy);

        permission.ResourceType.Should().Be(PermissionResourceType.File);
        permission.ResourceId.Should().Be(resourceId);
        permission.MemberId.Should().Be(memberId);
        permission.PermissionLevel.Should().Be(FilePermissionLevel.Edit);
        permission.FamilyId.Should().Be(familyId);
        permission.GrantedBy.Should().Be(grantedBy);
    }

    [Fact]
    public void Create_ForFile_ShouldRaiseFilePermissionChangedEvent()
    {
        var permission = FilePermission.Create(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(),
            UserId.New());

        permission.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FilePermissionChangedEvent>();
    }

    [Fact]
    public void Create_ForFolder_ShouldRaiseFolderPermissionChangedEvent()
    {
        var permission = FilePermission.Create(
            PermissionResourceType.Folder,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.Manage,
            FamilyId.New(),
            UserId.New());

        permission.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FolderPermissionChangedEvent>();
    }

    [Fact]
    public void UpdateLevel_ShouldChangePermissionLevel()
    {
        var permission = FilePermission.Create(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(),
            UserId.New());

        var changedBy = UserId.New();
        permission.UpdateLevel(FilePermissionLevel.Manage, changedBy);

        permission.PermissionLevel.Should().Be(FilePermissionLevel.Manage);
        permission.GrantedBy.Should().Be(changedBy);
    }

    [Fact]
    public void UpdateLevel_ShouldRaiseDomainEvent()
    {
        var permission = FilePermission.Create(
            PermissionResourceType.File,
            Guid.NewGuid(),
            UserId.New(),
            FilePermissionLevel.View,
            FamilyId.New(),
            UserId.New());

        permission.UpdateLevel(FilePermissionLevel.Edit, UserId.New());

        permission.DomainEvents.Should().HaveCount(2);
        permission.DomainEvents.Last().Should().BeOfType<FilePermissionChangedEvent>();
    }
}
