using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteTagCommandHandlerTests
{
    private static (DeleteTagCommandHandler handler, FakeTagRepository tagRepo, FakeFileTagRepository fileTagRepo) CreateHandler()
    {
        var tagRepo = new FakeTagRepository();
        var fileTagRepo = new FakeFileTagRepository();
        var handler = new DeleteTagCommandHandler(tagRepo, fileTagRepo);
        return (handler, tagRepo, fileTagRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTag()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo, _) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new DeleteTagCommand(tag.Id, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        tagRepo.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRemoveFileTagAssociations()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo, fileTagRepo) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        tagRepo.Tags.Add(tag);

        var fileTag1 = FileTag.Create(FileId.New(), tag.Id);
        var fileTag2 = FileTag.Create(FileId.New(), tag.Id);
        fileTagRepo.FileTags.Add(fileTag1);
        fileTagRepo.FileTags.Add(fileTag2);

        var command = new DeleteTagCommand(tag.Id, familyId);
        await handler.Handle(command, CancellationToken.None);

        fileTagRepo.FileTags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagNotFound()
    {
        var (handler, _, _) = CreateHandler();

        var command = new DeleteTagCommand(TagId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.TagNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenTagBelongsToDifferentFamily()
    {
        var (handler, tagRepo, _) = CreateHandler();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), FamilyId.New(), UserId.New());
        tagRepo.Tags.Add(tag);

        var command = new DeleteTagCommand(tag.Id, FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
