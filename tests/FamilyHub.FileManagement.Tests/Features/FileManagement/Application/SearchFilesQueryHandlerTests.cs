using FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SearchFilesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSearchResults()
    {
        var searchService = new FakeFileSearchService();
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        searchService.Results.Add(new FileSearchResultDto
        {
            Id = Guid.NewGuid(),
            Name = "vacation-photo.jpg",
            MimeType = "image/jpeg",
            Size = 1024,
            FolderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });

        var query = new SearchFilesQuery("vacation", FamilyId.New(), UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("vacation-photo.jpg");
    }

    [Fact]
    public async Task Handle_ShouldRecordRecentSearch()
    {
        var searchService = new FakeFileSearchService();
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        var userId = UserId.New();
        var query = new SearchFilesQuery("test", FamilyId.New(), userId);
        await handler.Handle(query, CancellationToken.None);

        recentRepo.Searches.Should().HaveCount(1);
        recentRepo.Searches.First().Query.Should().Be("test");
        recentRepo.Searches.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoMatches()
    {
        var searchService = new FakeFileSearchService();
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        var query = new SearchFilesQuery("nonexistent", FamilyId.New(), UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
