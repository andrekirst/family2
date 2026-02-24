using FamilyHub.Api.Features.FileManagement.Application.Queries.GetSecureNotes;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetSecureNotesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUserNotes()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new GetSecureNotesQueryHandler(noteRepo);
        var userId = UserId.New();
        var familyId = FamilyId.New();

        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Passwords,
            "note1", "content1", "iv1", "salt1", "sentinel1"));
        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Financial,
            "note2", "content2", "iv2", "salt2", "sentinel2"));

        var query = new GetSecureNotesQuery(userId, familyId, null);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredNotes()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new GetSecureNotesQueryHandler(noteRepo);
        var userId = UserId.New();
        var familyId = FamilyId.New();

        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Passwords,
            "pass1", "content1", "iv1", "salt1", "sentinel1"));
        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Financial,
            "fin1", "content2", "iv2", "salt2", "sentinel2"));
        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Passwords,
            "pass2", "content3", "iv3", "salt3", "sentinel3"));

        var query = new GetSecureNotesQuery(userId, familyId, NoteCategory.Passwords);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.Category.Should().Be("Passwords"));
    }

    [Fact]
    public async Task Handle_OtherUserNotes_ShouldNotReturn()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new GetSecureNotesQueryHandler(noteRepo);
        var familyId = FamilyId.New();

        noteRepo.Notes.Add(SecureNote.Create(
            familyId, UserId.New(), NoteCategory.Passwords,
            "note1", "content1", "iv1", "salt1", "sentinel1"));

        var query = new GetSecureNotesQuery(UserId.New(), familyId, null);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithAllFields()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new GetSecureNotesQueryHandler(noteRepo);
        var userId = UserId.New();
        var familyId = FamilyId.New();

        noteRepo.Notes.Add(SecureNote.Create(
            familyId, userId, NoteCategory.Medical,
            "enc-title", "enc-content", "my-iv", "my-salt", "my-sentinel"));

        var query = new GetSecureNotesQuery(userId, familyId, null);
        var result = await handler.Handle(query, CancellationToken.None);

        var dto = result.Single();
        dto.EncryptedTitle.Should().Be("enc-title");
        dto.EncryptedContent.Should().Be("enc-content");
        dto.Iv.Should().Be("my-iv");
        dto.Salt.Should().Be("my-salt");
        dto.Sentinel.Should().Be("my-sentinel");
        dto.Category.Should().Be("Medical");
    }
}
