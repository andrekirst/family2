using FamilyHub.Api.Features.FileManagement.Application.Queries.GetTags;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetTagsQueryHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IFileTagRepository _fileTagRepo = Substitute.For<IFileTagRepository>();
    private readonly GetTagsQueryHandler _handler;

    public GetTagsQueryHandlerTests()
    {
        _handler = new GetTagsQueryHandler(_tagRepo, _fileTagRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnTagsWithFileCounts()
    {
        var familyId = FamilyId.New();

        var tag1 = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        var tag2 = Tag.Create(TagName.From("Videos"), TagColor.From("#00FF00"), familyId, UserId.New());
        _tagRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([tag1, tag2]);

        _fileTagRepo.GetFileCountByTagIdAsync(tag1.Id, Arg.Any<CancellationToken>()).Returns(3);
        _fileTagRepo.GetFileCountByTagIdAsync(tag2.Id, Arg.Any<CancellationToken>()).Returns(1);

        var query = new GetTagsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);

        var photosTag = result.First(t => t.Name == "Photos");
        photosTag.FileCount.Should().Be(3);

        var videosTag = result.First(t => t.Name == "Videos");
        videosTag.FileCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyListWhenNoTags()
    {
        _tagRepo.GetByFamilyIdAsync(FamilyId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<Tag>());

        var query = new GetTagsQuery()
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnTagsForRequestedFamily()
    {
        var familyId = FamilyId.New();

        var tag = Tag.Create(TagName.From("Photos"), TagColor.From("#FF0000"), familyId, UserId.New());
        _tagRepo.GetByFamilyIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns([tag]);
        _fileTagRepo.GetFileCountByTagIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(0);

        var query = new GetTagsQuery()
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Photos");
    }
}
