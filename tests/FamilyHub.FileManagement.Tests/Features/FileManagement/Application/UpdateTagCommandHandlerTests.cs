using FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class UpdateTagCommandHandlerTests
{
    private static (UpdateTagCommandHandler handler, FakeTagRepository tagRepo) CreateHandler()
    {
        var tagRepo = new FakeTagRepository();
        var handler = new UpdateTagCommandHandler(tagRepo);
        return (handler, tagRepo);
    }

    [Fact]
    public async Task Handle_ShouldRenamTag()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new UpdateTagCommand(tag.Id, TagName.From("Images"), null, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.TagId.Should().Be(tag.Id);
        tag.Name.Value.Should().Be("Images");
    }

    [Fact]
    public async Task Handle_ShouldChangeColor()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new UpdateTagCommand(tag.Id, null, TagColor.From("#00FF00"), familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.TagId.Should().Be(tag.Id);
        tag.Color.Value.Should().Be("#00FF00");
    }

    [Fact]
    public async Task Handle_ShouldUpdateBothNameAndColor()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new UpdateTagCommand(tag.Id, TagName.From("Images"), TagColor.From("#00FF00"), familyId);
        await handler.Handle(command, CancellationToken.None);

        tag.Name.Value.Should().Be("Images");
        tag.Color.Value.Should().Be("#00FF00");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        var (handler, _) = CreateHandler();

        var command = new UpdateTagCommand(TagId.New(), TagName.From("New"), null, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagBelongsToDifferentFamily()
    {
        var (handler, tagRepo) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), FamilyId.New(), UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new UpdateTagCommand(tag.Id, TagName.From("New"), null, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenRenamingToDuplicateName()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo) = CreateHandler();

        var tag1 = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        var tag2 = Tag.Create(TagName.From("Videos"), TagColor.From("#00FF00"), familyId, UserId.New());
        tagRepo.Tags.Add(tag1);
        tagRepo.Tags.Add(tag2);

        var command = new UpdateTagCommand(tag2.Id, TagName.From("Photos"), null, familyId);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Conflict);
    }
}
