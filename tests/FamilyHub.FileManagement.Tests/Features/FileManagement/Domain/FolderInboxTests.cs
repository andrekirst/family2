using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class FolderInboxTests
{
    [Fact]
    public void CreateInbox_ShouldSetIsInboxTrue()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var rootFolderId = FolderId.New();

        var inbox = Folder.CreateInbox(rootFolderId, familyId, userId, DateTimeOffset.UtcNow);

        inbox.IsInbox.Should().BeTrue();
        inbox.Name.Value.Should().Be("Inbox");
        inbox.ParentFolderId.Should().Be(rootFolderId);
        inbox.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public void Create_ShouldSetIsInboxFalse()
    {
        var folder = Folder.Create(
            FamilyHub.Api.Features.FileManagement.Domain.ValueObjects.FileName.From("Photos"),
            FolderId.New(),
            "/root/",
            FamilyId.New(),
            UserId.New(), DateTimeOffset.UtcNow);

        folder.IsInbox.Should().BeFalse();
    }

    [Fact]
    public void CreateRoot_ShouldSetIsInboxFalse()
    {
        var root = Folder.CreateRoot(FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);

        root.IsInbox.Should().BeFalse();
    }
}
