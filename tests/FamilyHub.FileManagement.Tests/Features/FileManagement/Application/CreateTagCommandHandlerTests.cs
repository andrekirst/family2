using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly CreateTagCommandHandler _handler;

    public CreateTagCommandHandlerTests()
    {
        _handler = new CreateTagCommandHandler(_tagRepo);
    }

    [Fact]
    public async Task Handle_ShouldCreateTag()
    {
        var familyId = FamilyId.New();

        _tagRepo.GetByNameAsync(TagName.From("Photos"), familyId, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#FF0000"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TagId.Value.Should().NotBe(Guid.Empty);
        await _tagRepo.Received(1).AddAsync(
            Arg.Is<Tag>(t => t.Name.Value == "Photos" && t.Color.Value == "#FF0000"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDuplicateNameInFamily()
    {
        var familyId = FamilyId.New();

        var existingTag = Tag.Create(
            TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        _tagRepo.GetByNameAsync(TagName.From("Photos"), familyId, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#00FF00"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };

        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Conflict);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameNameInDifferentFamilies()
    {
        var family2 = FamilyId.New();

        _tagRepo.GetByNameAsync(TagName.From("Photos"), family2, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        var command = new CreateTagCommand(
            TagName.From("Photos"),
            TagColor.From("#00FF00"))
        {
            FamilyId = family2,
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TagId.Value.Should().NotBe(Guid.Empty);
        await _tagRepo.Received(1).AddAsync(Arg.Any<Tag>(), Arg.Any<CancellationToken>());
    }
}
