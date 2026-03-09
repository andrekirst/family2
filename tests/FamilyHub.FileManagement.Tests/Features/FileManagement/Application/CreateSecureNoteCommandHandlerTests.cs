using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateSecureNoteCommandHandlerTests
{
    private readonly ISecureNoteRepository _noteRepo = Substitute.For<ISecureNoteRepository>();
    private readonly CreateSecureNoteCommandHandler _handler;

    public CreateSecureNoteCommandHandlerTests()
    {
        _handler = new CreateSecureNoteCommandHandler(_noteRepo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldCreateNote()
    {
        var command = new CreateSecureNoteCommand(
            NoteCategory.Passwords,
            "enc-title",
            "enc-content",
            "iv-123",
            "salt-456",
            "sentinel-789")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NoteId.Should().NotBe(Guid.Empty);
        await _noteRepo.Received(1).AddAsync(
            Arg.Is<SecureNote>(n =>
                n.EncryptedTitle == "enc-title" &&
                n.Category == NoteCategory.Passwords),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldStoreSaltAndSentinel()
    {
        var command = new CreateSecureNoteCommand(
            NoteCategory.Financial,
            "enc-title",
            "enc-content",
            "iv-123",
            "my-salt",
            "my-sentinel")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        await _handler.Handle(command, CancellationToken.None);

        await _noteRepo.Received(1).AddAsync(
            Arg.Is<SecureNote>(n =>
                n.Salt == "my-salt" &&
                n.Sentinel == "my-sentinel"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleCalls_ShouldCreateSeparateNotes()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        await _handler.Handle(new CreateSecureNoteCommand(
            NoteCategory.Passwords,
            "note1",
            "content1",
            "iv1",
            "salt1",
            "sentinel1")
        {
            FamilyId = familyId,
            UserId = userId
        }, CancellationToken.None);

        await _handler.Handle(new CreateSecureNoteCommand(
            NoteCategory.Medical,
            "note2",
            "content2",
            "iv2",
            "salt2",
            "sentinel2")
        {
            FamilyId = familyId,
            UserId = userId
        }, CancellationToken.None);

        await _noteRepo.Received(2).AddAsync(Arg.Any<SecureNote>(), Arg.Any<CancellationToken>());
    }
}
