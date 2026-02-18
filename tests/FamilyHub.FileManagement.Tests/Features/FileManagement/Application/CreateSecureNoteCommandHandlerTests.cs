using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateSecureNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateNote()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new CreateSecureNoteCommandHandler(noteRepo);

        var command = new CreateSecureNoteCommand(
            FamilyId.New(),
            UserId.New(),
            NoteCategory.Passwords,
            "enc-title",
            "enc-content",
            "iv-123",
            "salt-456",
            "sentinel-789");

        var result = await handler.Handle(command, CancellationToken.None);

        result.NoteId.Should().NotBe(Guid.Empty);
        noteRepo.Notes.Should().HaveCount(1);
        noteRepo.Notes.First().EncryptedTitle.Should().Be("enc-title");
        noteRepo.Notes.First().Category.Should().Be(NoteCategory.Passwords);
    }

    [Fact]
    public async Task Handle_ShouldStoreSaltAndSentinel()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new CreateSecureNoteCommandHandler(noteRepo);

        var command = new CreateSecureNoteCommand(
            FamilyId.New(),
            UserId.New(),
            NoteCategory.Financial,
            "enc-title",
            "enc-content",
            "iv-123",
            "my-salt",
            "my-sentinel");

        await handler.Handle(command, CancellationToken.None);

        var note = noteRepo.Notes.First();
        note.Salt.Should().Be("my-salt");
        note.Sentinel.Should().Be("my-sentinel");
    }

    [Fact]
    public async Task Handle_MultipleCalls_ShouldCreateSeparateNotes()
    {
        var noteRepo = new FakeSecureNoteRepository();
        var handler = new CreateSecureNoteCommandHandler(noteRepo);
        var userId = UserId.New();
        var familyId = FamilyId.New();

        await handler.Handle(new CreateSecureNoteCommand(
            familyId, userId, NoteCategory.Passwords,
            "note1", "content1", "iv1", "salt1", "sentinel1"), CancellationToken.None);

        await handler.Handle(new CreateSecureNoteCommand(
            familyId, userId, NoteCategory.Medical,
            "note2", "content2", "iv2", "salt2", "sentinel2"), CancellationToken.None);

        noteRepo.Notes.Should().HaveCount(2);
    }
}
