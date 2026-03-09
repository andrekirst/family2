using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteSecureNoteCommandHandlerTests
{
    private readonly ISecureNoteRepository _noteRepo = Substitute.For<ISecureNoteRepository>();
    private readonly DeleteSecureNoteCommandHandler _handler;

    public DeleteSecureNoteCommandHandlerTests()
    {
        _handler = new DeleteSecureNoteCommandHandler(_noteRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteNote()
    {
        var userId = UserId.New();
        var note = SecureNote.Create(
            FamilyId.New(), userId, NoteCategory.Passwords,
            "title", "content", "iv", "salt", "sentinel", DateTimeOffset.UtcNow);
        _noteRepo.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var command = new DeleteSecureNoteCommand(note.Id)
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _noteRepo.Received(1).RemoveAsync(note, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoteNotFound_ShouldThrow()
    {
        _noteRepo.GetByIdAsync(SecureNoteId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((SecureNote?)null);

        var command = new DeleteSecureNoteCommand(SecureNoteId.New())
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
            FamilyId.New(), UserId.New(), NoteCategory.Medical,
            "title", "content", "iv", "salt", "sentinel", DateTimeOffset.UtcNow);
        _noteRepo.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var command = new DeleteSecureNoteCommand(note.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Secure note not found*");
    }
}
