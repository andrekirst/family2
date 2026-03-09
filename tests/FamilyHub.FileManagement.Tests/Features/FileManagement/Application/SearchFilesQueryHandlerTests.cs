using FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;
using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class SearchFilesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSearchResults()
    {
        var searchService = Substitute.For<IFileSearchService>();
        var recentRepo = Substitute.For<IRecentSearchRepository>();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        var familyId = FamilyId.New();
        var results = new List<FileSearchResultDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "vacation-photo.jpg",
                MimeType = "image/jpeg",
                Size = 1024,
                FolderId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };
        searchService.SearchAsync("vacation", familyId, null, "relevance", 0, 20, Arg.Any<CancellationToken>())
            .Returns(results);

        var query = new SearchFilesQuery("vacation")
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("vacation-photo.jpg");
    }

    [Fact]
    public async Task Handle_ShouldRecordRecentSearch()
    {
        var searchService = Substitute.For<IFileSearchService>();
        var recentRepo = Substitute.For<IRecentSearchRepository>();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        var userId = UserId.New();
        var familyId = FamilyId.New();
        searchService.SearchAsync("test", familyId, null, "relevance", 0, 20, Arg.Any<CancellationToken>())
            .Returns(new List<FileSearchResultDto>());

        var query = new SearchFilesQuery("test")
        {
            FamilyId = familyId,
            UserId = userId
        };
        await handler.Handle(query, CancellationToken.None);

        await recentRepo.Received(1).AddAsync(
            Arg.Is<Api.Features.FileManagement.Domain.Entities.RecentSearch>(s => s.Query == "test" && s.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoMatches()
    {
        var searchService = Substitute.For<IFileSearchService>();
        var recentRepo = Substitute.For<IRecentSearchRepository>();
        var handler = new SearchFilesQueryHandler(searchService, recentRepo);

        var familyId = FamilyId.New();
        searchService.SearchAsync("nonexistent", familyId, null, "relevance", 0, 20, Arg.Any<CancellationToken>())
            .Returns(new List<FileSearchResultDto>());

        var query = new SearchFilesQuery("nonexistent")
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
