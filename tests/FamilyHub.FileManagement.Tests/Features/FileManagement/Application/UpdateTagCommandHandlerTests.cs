using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly UpdateTagCommandHandler _handler;

    public UpdateTagCommandHandlerTests()
    {
        _handler = new UpdateTagCommandHandler(_tagRepo);
    }

    [Fact]
    public async Task Handle_ShouldRenamTag()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepo.GetByNameAsync(TagName.From("Images"), familyId, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        var command = new UpdateTagCommand(tag.Id, TagName.From("Images"), null)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.TagId.Should().Be(tag.Id);
        tag.Name.Value.Should().Be("Images");
    }

    [Fact]
    public async Task Handle_ShouldChangeColor()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new UpdateTagCommand(tag.Id, null, TagColor.From("#00FF00"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.TagId.Should().Be(tag.Id);
        tag.Color.Value.Should().Be("#00FF00");
    }

    [Fact]
    public async Task Handle_ShouldUpdateBothNameAndColor()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepo.GetByNameAsync(TagName.From("Images"), familyId, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        var command = new UpdateTagCommand(tag.Id, TagName.From("Images"), TagColor.From("#00FF00"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        await _handler.Handle(command, CancellationToken.None);

        tag.Name.Value.Should().Be("Images");
        tag.Color.Value.Should().Be("#00FF00");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        _tagRepo.GetByIdAsync(TagId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Tag?)null);

        var command = new UpdateTagCommand(TagId.New(), TagName.From("New"), null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagBelongsToDifferentFamily()
    {
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new UpdateTagCommand(tag.Id, TagName.From("New"), null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenRenamingToDuplicateName()
    {
        var familyId = FamilyId.New();
        var tag1 = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        var tag2 = Tag.Create(TagName.From("Videos"), TagColor.From("#00FF00"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag2.Id, Arg.Any<CancellationToken>()).Returns(tag2);
        _tagRepo.GetByNameAsync(TagName.From("Photos"), familyId, Arg.Any<CancellationToken>())
            .Returns(tag1);

        var command = new UpdateTagCommand(tag2.Id, TagName.From("Photos"), null)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Conflict);
    }
}
