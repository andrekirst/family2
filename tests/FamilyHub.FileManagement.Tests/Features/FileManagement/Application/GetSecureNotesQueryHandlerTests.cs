using FamilyHub.Api.Features.FileManagement.Application.Queries.GetSecureNotes;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetSecureNotesQueryHandlerTests
{
    private readonly ISecureNoteRepository _noteRepo = Substitute.For<ISecureNoteRepository>();
    private readonly GetSecureNotesQueryHandler _handler;

    public GetSecureNotesQueryHandlerTests()
    {
        _handler = new GetSecureNotesQueryHandler(_noteRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNotes()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        _noteRepo.GetByUserIdAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns([
                SecureNote.Create(familyId, userId, NoteCategory.Passwords, "note1", "content1", "iv1", "salt1", "sentinel1", DateTimeOffset.UtcNow),
                SecureNote.Create(familyId, userId, NoteCategory.Financial, "note2", "content2", "iv2", "salt2", "sentinel2", DateTimeOffset.UtcNow)
            ]);

        var query = new GetSecureNotesQuery(null)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnFilteredNotes()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        _noteRepo.GetByUserIdAndCategoryAsync(userId, familyId, NoteCategory.Passwords, Arg.Any<CancellationToken>())
            .Returns([
                SecureNote.Create(familyId, userId, NoteCategory.Passwords, "pass1", "content1", "iv1", "salt1", "sentinel1", DateTimeOffset.UtcNow),
                SecureNote.Create(familyId, userId, NoteCategory.Passwords, "pass2", "content3", "iv3", "salt3", "sentinel3", DateTimeOffset.UtcNow)
            ]);

        var query = new GetSecureNotesQuery(NoteCategory.Passwords)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.Category.Should().Be("Passwords"));
    }

    [Fact]
    public async Task Handle_OtherUserNotes_ShouldNotReturn()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();

        _noteRepo.GetByUserIdAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns(new List<SecureNote>());

        var query = new GetSecureNotesQuery(null)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithAllFields()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        _noteRepo.GetByUserIdAsync(userId, familyId, Arg.Any<CancellationToken>())
            .Returns([
                SecureNote.Create(familyId, userId, NoteCategory.Medical, "enc-title", "enc-content", "my-iv", "my-salt", "my-sentinel", DateTimeOffset.UtcNow)
            ]);

        var query = new GetSecureNotesQuery(null)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Single();
        dto.EncryptedTitle.Should().Be("enc-title");
        dto.EncryptedContent.Should().Be("enc-content");
        dto.Iv.Should().Be("my-iv");
        dto.Salt.Should().Be("my-salt");
        dto.Sentinel.Should().Be("my-sentinel");
        dto.Category.Should().Be("Medical");
    }
}
