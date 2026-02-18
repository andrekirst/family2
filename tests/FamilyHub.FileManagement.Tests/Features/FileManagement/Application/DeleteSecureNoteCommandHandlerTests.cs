using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteSecureNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteNote()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new DeleteSecureNoteCommandHandler(noteRepo);
        var userId = UserId.New();

        var note = SecureNote.Create(
            FamilyId.New(), userId, NoteCategory.Passwords,
            "title", "content", "iv", "salt", "sentinel");
        noteRepo.Notes.Add(note);

        var command = new DeleteSecureNoteCommand(note.Id, userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        noteRepo.Notes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoteNotFound_ShouldThrow()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new DeleteSecureNoteCommandHandler(noteRepo);

        var command = new DeleteSecureNoteCommand(SecureNoteId.New(), UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }

    [Fact]
    public async Task Handle_WrongUser_ShouldThrow()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new DeleteSecureNoteCommandHandler(noteRepo);

        var note = SecureNote.Create(
            FamilyId.New(), UserId.New(), NoteCategory.Medical,
            "title", "content", "iv", "salt", "sentinel");
        noteRepo.Notes.Add(note);

        var command = new DeleteSecureNoteCommand(note.Id, UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }
}
