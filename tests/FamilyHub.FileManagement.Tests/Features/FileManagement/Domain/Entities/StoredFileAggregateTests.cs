using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.Entities;

public class StoredFileAggregateTests
{
    private static StoredFile CreateTestFile(
        FolderId? folderId = null,
        FamilyId? familyId = null,
        UserId? uploadedBy = null)
    {
        return StoredFile.Create(
            FileName.From("test-document.pdf"),
            MimeType.From("application/pdf"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            folderId ?? FolderId.New(),
            familyId ?? FamilyId.New(),
            uploadedBy ?? UserId.New());
    }

    [Fact]
    public void Create_ShouldCreateFileWithValidData()
    {
        var file = CreateTestFile();

        file.Should().NotBeNull();
        file.Id.Value.Should().NotBe(Guid.Empty);
        file.Name.Value.Should().Be("test-document.pdf");
        file.MimeType.Value.Should().Be("application/pdf");
        file.Size.Value.Should().Be(1024);
        file.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        file.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseFileUploadedEvent()
    {
        var file = CreateTestFile();

        file.DomainEvents.Should().HaveCount(1);
        var domainEvent = file.DomainEvents.First();
        domainEvent.Should().BeOfType<FileUploadedEvent>();

        var evt = (FileUploadedEvent)domainEvent;
        evt.FileId.Should().Be(file.Id);
        evt.FamilyId.Should().Be(file.FamilyId);
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndRaiseEvent()
    {
        var file = CreateTestFile();
        var userId = UserId.New();
        var newName = FileName.From("renamed-document.pdf");

        file.Rename(newName, userId);

        file.Name.Should().Be(newName);
        file.DomainEvents.Should().HaveCount(2);
        file.DomainEvents.Last().Should().BeOfType<FileRenamedEvent>();

        var evt = (FileRenamedEvent)file.DomainEvents.Last();
        evt.OldName.Value.Should().Be("test-document.pdf");
        evt.NewName.Value.Should().Be("renamed-document.pdf");
        evt.RenamedBy.Should().Be(userId);
    }

    [Fact]
    public void Rename_ShouldUpdateTimestamp()
    {
        var file = CreateTestFile();
        var originalUpdatedAt = file.UpdatedAt;

        file.Rename(FileName.From("new-name.pdf"), UserId.New());

        file.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void MoveTo_ShouldUpdateFolderIdAndRaiseEvent()
    {
        var originalFolderId = FolderId.New();
        var newFolderId = FolderId.New();
        var userId = UserId.New();
        var file = CreateTestFile(folderId: originalFolderId);

        file.MoveTo(newFolderId, userId);

        file.FolderId.Should().Be(newFolderId);
        file.DomainEvents.Should().HaveCount(2);
        file.DomainEvents.Last().Should().BeOfType<FileMovedEvent>();

        var evt = (FileMovedEvent)file.DomainEvents.Last();
        evt.FromFolderId.Should().Be(originalFolderId);
        evt.ToFolderId.Should().Be(newFolderId);
        evt.MovedBy.Should().Be(userId);
    }

    [Fact]
    public void MoveTo_ShouldUpdateTimestamp()
    {
        var file = CreateTestFile();
        var originalUpdatedAt = file.UpdatedAt;

        file.MoveTo(FolderId.New(), UserId.New());

        file.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkDeleted_ShouldRaiseFileDeletedEvent()
    {
        var file = CreateTestFile();
        var userId = UserId.New();

        file.MarkDeleted(userId);

        file.DomainEvents.Should().HaveCount(2);
        file.DomainEvents.Last().Should().BeOfType<FileDeletedEvent>();

        var evt = (FileDeletedEvent)file.DomainEvents.Last();
        evt.FileId.Should().Be(file.Id);
        evt.FamilyId.Should().Be(file.FamilyId);
    }
}
