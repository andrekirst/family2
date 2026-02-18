using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateTagCommandHandlerTests
{
    private static (CreateTagCommandHandler handler, FakeTagRepository tagRepo) CreateHandler()
    {
        var tagRepo = new FakeTagRepository();
        var handler = new CreateTagCommandHandler(tagRepo);
        return (handler, tagRepo);
    }

    [Fact]
    public async Task Handle_ShouldCreateTag()
    {
        var (handler, tagRepo) = CreateHandler();
        var familyId = FamilyId.New();

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#FF0000"),
            familyId,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.TagId.Value.Should().NotBe(Guid.Empty);
        tagRepo.Tags.Should().HaveCount(1);
        tagRepo.Tags.First().Name.Value.Should().Be("Photos");
        tagRepo.Tags.First().Color.Value.Should().Be("#FF0000");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDuplicateNameInFamily()
    {
        var (handler, tagRepo) = CreateHandler();
        var familyId = FamilyId.New();

        var existingTag = Api.Features.FileManagement.Domain.Entities.Tag.Create(
            TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(existingTag);

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#00FF00"),
            familyId,
            UserId.New());

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Conflict);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameNameInDifferentFamilies()
    {
        var (handler, tagRepo) = CreateHandler();
        var family1 = FamilyId.New();
        var family2 = FamilyId.New();

        var existingTag = Api.Features.FileManagement.Domain.Entities.Tag.Create(
            TagName.From("Photos"), TagColor.From("#FF0000"), family1, UserId.New());
        tagRepo.Tags.Add(existingTag);

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#00FF00"),
            family2,
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.TagId.Value.Should().NotBe(Guid.Empty);
        tagRepo.Tags.Should().HaveCount(2);
    }
}
