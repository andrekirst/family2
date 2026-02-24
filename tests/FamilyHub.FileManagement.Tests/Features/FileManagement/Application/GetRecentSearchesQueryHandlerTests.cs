using FamilyHub.Api.Features.FileManagement.Application.Queries.GetRecentSearches;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetRecentSearchesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRecentSearches()
    {
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new GetRecentSearchesQueryHandler(recentRepo);

        var userId = UserId.New();
        recentRepo.Searches.Add(RecentSearch.Create(userId, "photos"));
        recentRepo.Searches.Add(RecentSearch.Create(userId, "documents"));

        var query = new GetRecentSearchesQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoSearches()
    {
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new GetRecentSearchesQueryHandler(recentRepo);

        var query = new GetRecentSearchesQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnCurrentUserSearches()
    {
        var recentRepo = new FakeRecentSearchRepository();
        var handler = new GetRecentSearchesQueryHandler(recentRepo);

        var userId = UserId.New();
        recentRepo.Searches.Add(RecentSearch.Create(userId, "mine"));
        recentRepo.Searches.Add(RecentSearch.Create(UserId.New(), "other"));

        var query = new GetRecentSearchesQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Query.Should().Be("mine");
    }
}
