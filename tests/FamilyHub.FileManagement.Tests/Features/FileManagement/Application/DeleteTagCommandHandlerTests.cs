using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IFileTagRepository _fileTagRepo = Substitute.For<IFileTagRepository>();
    private readonly DeleteTagCommandHandler _handler;

    public DeleteTagCommandHandlerTests()
    {
        _handler = new DeleteTagCommandHandler(_tagRepo, _fileTagRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTag()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new DeleteTagCommand(tag.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _tagRepo.Received(1).RemoveAsync(tag, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveFileTagAssociations()
    {
        var familyId = FamilyId.New();
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new DeleteTagCommand(tag.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        await _handler.Handle(command, CancellationToken.None);

        await _fileTagRepo.Received(1).RemoveByTagIdAsync(tag.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        _tagRepo.GetByIdAsync(TagId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Tag?)null);

        var command = new DeleteTagCommand(TagId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagBelongsToDifferentFamily()
    {
        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);

        var command = new DeleteTagCommand(tag.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
