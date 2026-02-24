using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateSecureNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateNote()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new UpdateSecureNoteCommandHandler(noteRepo);
        var userId = UserId.New();

        var note = SecureNote.Create(
            FamilyId.New(), userId, NoteCategory.Passwords,
            "old-title", "old-content", "old-iv", "salt", "sentinel");
        noteRepo.Notes.Add(note);

        var command = new UpdateSecureNoteCommand(
            note.Id, userId, NoteCategory.Financial,
            "new-title", "new-content", "new-iv");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        note.Category.Should().Be(NoteCategory.Financial);
        note.EncryptedTitle.Should().Be("new-title");
        note.EncryptedContent.Should().Be("new-content");
        note.Iv.Should().Be("new-iv");
    }

    [Fact]
    public async Task Handle_NoteNotFound_ShouldThrow()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new UpdateSecureNoteCommandHandler(noteRepo);

        var command = new UpdateSecureNoteCommand(
            SecureNoteId.New(), UserId.New(), NoteCategory.Personal,
            "title", "content", "iv");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }

    [Fact]
    public async Task Handle_WrongUser_ShouldThrow()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new UpdateSecureNoteCommandHandler(noteRepo);

        var note = SecureNote.Create(
            FamilyId.New(), UserId.New(), NoteCategory.Passwords,
            "title", "content", "iv", "salt", "sentinel");
        noteRepo.Notes.Add(note);

        var command = new UpdateSecureNoteCommand(
            note.Id, UserId.New(), NoteCategory.Personal,
            "new-title", "new-content", "new-iv");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }
}
