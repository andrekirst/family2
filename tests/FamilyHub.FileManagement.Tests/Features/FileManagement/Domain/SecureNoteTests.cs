using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class SecureNoteTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        var note = SecureNote.Create(
            familyId, userId, NoteCategory.Passwords,
            "enc-title", "enc-content", "iv-123", "salt-456", "sentinel-789");

        note.FamilyId.Should().Be(familyId);
        note.UserId.Should().Be(userId);
        note.Category.Should().Be(NoteCategory.Passwords);
        note.EncryptedTitle.Should().Be("enc-title");
        note.EncryptedContent.Should().Be("enc-content");
        note.Iv.Should().Be("iv-123");
        note.Salt.Should().Be("salt-456");
        note.Sentinel.Should().Be("sentinel-789");
        note.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        note.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldRaiseSecureNoteCreatedEvent()
    {
        var familyId = FamilyId.New();
        var note = SecureNote.Create(
            familyId, UserId.New(), NoteCategory.Financial,
            "enc-title", "enc-content", "iv", "salt", "sentinel");

        var domainEvent = note.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SecureNoteCreatedEvent>().Subject;
        domainEvent.NoteId.Should().Be(note.Id);
        domainEvent.Category.Should().Be(NoteCategory.Financial);
        domainEvent.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public void Update_ShouldChangeEncryptedFields()
    {
        var note = SecureNote.Create(
            FamilyId.New(), UserId.New(), NoteCategory.Passwords,
            "old-title", "old-content", "old-iv", "salt", "sentinel");

        var originalUpdatedAt = note.UpdatedAt;

        note.Update(NoteCategory.Medical, "new-title", "new-content", "new-iv");

        note.Category.Should().Be(NoteCategory.Medical);
        note.EncryptedTitle.Should().Be("new-title");
        note.EncryptedContent.Should().Be("new-content");
        note.Iv.Should().Be("new-iv");
        // Salt and sentinel should not change on update
        note.Salt.Should().Be("salt");
        note.Sentinel.Should().Be("sentinel");
        note.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkDeleted_ShouldRaiseSecureNoteDeletedEvent()
    {
        var familyId = FamilyId.New();
        var note = SecureNote.Create(
            familyId, UserId.New(), NoteCategory.Personal,
            "enc-title", "enc-content", "iv", "salt", "sentinel");

        note.ClearDomainEvents();
        note.MarkDeleted();

        var domainEvent = note.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SecureNoteDeletedEvent>().Subject;
        domainEvent.NoteId.Should().Be(note.Id);
        domainEvent.FamilyId.Should().Be(familyId);
    }

    [Theory]
    [InlineData(NoteCategory.Passwords)]
    [InlineData(NoteCategory.Financial)]
    [InlineData(NoteCategory.Medical)]
    [InlineData(NoteCategory.Personal)]
    [InlineData(NoteCategory.Custom)]
    public void Create_AllCategories_ShouldWork(NoteCategory category)
    {
        var note = SecureNote.Create(
            FamilyId.New(), UserId.New(), category,
            "title", "content", "iv", "salt", "sentinel");

        note.Category.Should().Be(category);
    }
}
