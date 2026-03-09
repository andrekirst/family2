using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateSecureNoteCommandHandlerTests
{
    private readonly ISecureNoteRepository _noteRepo = Substitute.For<ISecureNoteRepository>();
    private readonly UpdateSecureNoteCommandHandler _handler;

    public UpdateSecureNoteCommandHandlerTests()
    {
        _handler = new UpdateSecureNoteCommandHandler(_noteRepo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldUpdateNote()
    {
        var userId = UserId.New();
        var note = SecureNote.Create(
            FamilyId.New(), userId, NoteCategory.Passwords,
            "old-title", "old-content", "old-iv", "salt", "sentinel", DateTimeOffset.UtcNow);
        _noteRepo.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var command = new UpdateSecureNoteCommand(
            note.Id, NoteCategory.Financial,
            "new-title", "new-content", "new-iv")
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        note.Category.Should().Be(NoteCategory.Financial);
        note.EncryptedTitle.Should().Be("new-title");
        note.EncryptedContent.Should().Be("new-content");
        note.Iv.Should().Be("new-iv");
    }

    [Fact]
    public async Task Handle_NoteNotFound_ShouldThrow()
    {
        _noteRepo.GetByIdAsync(SecureNoteId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((SecureNote?)null);

        var command = new UpdateSecureNoteCommand(
            SecureNoteId.New(), NoteCategory.Personal,
            "title", "content", "iv")
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }

    [Fact]
    public async Task Handle_WrongUser_ShouldThrow()
    {
        var note = SecureNote.Create(
            FamilyId.New(), UserId.New(), NoteCategory.Passwords,
            "title", "content", "iv", "salt", "sentinel", DateTimeOffset.UtcNow);
        _noteRepo.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var command = new UpdateSecureNoteCommand(
            note.Id, NoteCategory.Personal,
            "new-title", "new-content", "new-iv")
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }
}
