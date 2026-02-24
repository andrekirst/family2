using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.Entities;

public class FolderAggregateTests
{
    [Fact]
    public void Create_ShouldCreateFolderWithValidData()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var parentId = FolderId.New();

        var folder = Folder.Create(
            FileName.From("Documents"),
            parentId,
            $"/{parentId.Value}/",
            familyId,
            userId);

        folder.Should().NotBeNull();
        folder.Id.Value.Should().NotBe(Guid.Empty);
        folder.Name.Value.Should().Be("Documents");
        folder.ParentFolderId.Should().Be(parentId);
        folder.FamilyId.Should().Be(familyId);
        folder.CreatedBy.Should().Be(userId);
        folder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseFolderCreatedEvent()
    {
        var folder = Folder.Create(
            FileName.From("Photos"),
            null,
            "/",
            FamilyId.New(),
            UserId.New());

        folder.DomainEvents.Should().HaveCount(1);
        folder.DomainEvents.First().Should().BeOfType<FolderCreatedEvent>();

        var evt = (FolderCreatedEvent)folder.DomainEvents.First();
        evt.FolderId.Should().Be(folder.Id);
        evt.FolderName.Value.Should().Be("Photos");
    }

    [Fact]
    public void CreateRoot_ShouldCreateRootFolderWithCorrectDefaults()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var root = Folder.CreateRoot(familyId, userId);

        root.Name.Value.Should().Be("Root");
        root.ParentFolderId.Should().BeNull();
        root.MaterializedPath.Should().Be("/");
        root.FamilyId.Should().Be(familyId);
        root.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public void CreateRoot_ShouldNotRaiseDomainEvent()
    {
        var root = Folder.CreateRoot(FamilyId.New(), UserId.New());

        root.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var folder = Folder.Create(
            FileName.From("Old Name"),
            null,
            "/",
            FamilyId.New(),
            UserId.New());

        folder.Rename(FileName.From("New Name"));

        folder.Name.Value.Should().Be("New Name");
    }

    [Fact]
    public void Rename_ShouldUpdateTimestamp()
    {
        var folder = Folder.Create(
            FileName.From("Folder"),
            null,
            "/",
            FamilyId.New(),
            UserId.New());
        var originalUpdatedAt = folder.UpdatedAt;

        folder.Rename(FileName.From("Renamed"));

        folder.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateMaterializedPath_ShouldUpdatePath()
    {
        var folder = Folder.Create(
            FileName.From("Sub"),
            FolderId.New(),
            "/parent/",
            FamilyId.New(),
            UserId.New());

        folder.UpdateMaterializedPath("/new-parent/");

        folder.MaterializedPath.Should().Be("/new-parent/");
    }

    [Fact]
    public void MoveTo_ShouldUpdateParentAndPath()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var oldParentId = FolderId.New();
        var newParentId = FolderId.New();

        var folder = Folder.Create(
            FileName.From("Movable"),
            oldParentId,
            $"/{oldParentId.Value}/",
            familyId,
            userId);

        folder.MoveTo(newParentId, $"/{newParentId.Value}/", userId);

        folder.ParentFolderId.Should().Be(newParentId);
        folder.MaterializedPath.Should().Be($"/{newParentId.Value}/");
    }

    [Fact]
    public void MoveTo_ShouldRaiseFolderMovedEvent()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var oldParentId = FolderId.New();
        var newParentId = FolderId.New();

        var folder = Folder.Create(
            FileName.From("Movable"),
            oldParentId,
            $"/{oldParentId.Value}/",
            familyId,
            userId);

        folder.MoveTo(newParentId, $"/{newParentId.Value}/", userId);

        folder.DomainEvents.Should().HaveCount(2); // Created + Moved
        folder.DomainEvents.Last().Should().BeOfType<FolderMovedEvent>();

        var evt = (FolderMovedEvent)folder.DomainEvents.Last();
        evt.OldParentFolderId.Should().Be(oldParentId);
        evt.NewParentFolderId.Should().Be(newParentId);
        evt.MovedBy.Should().Be(userId);
    }

    [Fact]
    public void MarkDeleted_ShouldRaiseFolderDeletedEvent()
    {
        var folder = Folder.Create(
            FileName.From("ToDelete"),
            null,
            "/",
            FamilyId.New(),
            UserId.New());
        var userId = UserId.New();

        folder.MarkDeleted(userId);

        folder.DomainEvents.Should().HaveCount(2);
        folder.DomainEvents.Last().Should().BeOfType<FolderDeletedEvent>();

        var evt = (FolderDeletedEvent)folder.DomainEvents.Last();
        evt.FolderId.Should().Be(folder.Id);
        evt.DeletedBy.Should().Be(userId);
    }
}
