using FamilyHub.Api.Features.FileManagement.Application.Queries.GetTags;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetTagsQueryHandlerTests
{
    private static (GetTagsQueryHandler handler, FakeTagRepository tagRepo, FakeFileTagRepository fileTagRepo) CreateHandler()
    {
        var tagRepo = new FakeTagRepository();
        var fileTagRepo = new FakeFileTagRepository();
        var handler = new GetTagsQueryHandler(tagRepo, fileTagRepo);
        return (handler, tagRepo, fileTagRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnTagsWithFileCounts()
    {
        var familyId = FamilyId.New();
        var (handler, tagRepo, fileTagRepo) = CreateHandler();

        var tag1 = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        var tag2 = Tag.Create(TagName.From("Videos"), TagColor.From("#00FF00"), familyId, UserId.New());
        tagRepo.Tags.Add(tag1);
        tagRepo.Tags.Add(tag2);

        // tag1 has 3 files, tag2 has 1 file
        fileTagRepo.FileTags.Add(FileTag.Create(FileId.New(), tag1.Id));
        fileTagRepo.FileTags.Add(FileTag.Create(FileId.New(), tag1.Id));
        fileTagRepo.FileTags.Add(FileTag.Create(FileId.New(), tag1.Id));
        fileTagRepo.FileTags.Add(FileTag.Create(FileId.New(), tag2.Id));

        var query = new GetTagsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);

        var photosTag = result.First(t => t.Name == "Photos");
        photosTag.FileCount.Should().Be(3);

        var videosTag = result.First(t => t.Name == "Videos");
        videosTag.FileCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoTags()
    {
        var (handler, _, _) = CreateHandler();

        var query = new GetTagsQuery(FamilyId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnTagsForRequestedFamily()
    {
        var familyId = FamilyId.New();
        var otherFamilyId = FamilyId.New();
        var (handler, tagRepo, _) = CreateHandler();

        tagRepo.Tags.Add(Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New()));
        tagRepo.Tags.Add(Tag.Create(TagName.From("Other"), TagColor.From("#00FF00"), otherFamilyId, UserId.New()));

        var query = new GetTagsQuery(familyId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Photos");
    }
}
