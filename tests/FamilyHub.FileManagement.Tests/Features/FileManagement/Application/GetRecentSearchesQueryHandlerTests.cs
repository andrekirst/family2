using FamilyHub.Api.Features.FileManagement.Application.Queries.GetRecentSearches;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetRecentSearchesQueryHandlerTests
{
    private readonly IRecentSearchRepository _recentRepo = Substitute.For<IRecentSearchRepository>();
    private readonly GetRecentSearchesQueryHandler _handler;

    public GetRecentSearchesQueryHandlerTests()
    {
        _handler = new GetRecentSearchesQueryHandler(_recentRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnRecentSearches()
    {
        var userId = UserId.New();
        _recentRepo.GetByUserIdAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([
                RecentSearch.Create(userId, "photos", DateTimeOffset.UtcNow),
                RecentSearch.Create(userId, "documents", DateTimeOffset.UtcNow)
            ]);

        var query = new GetRecentSearchesQuery()
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoSearches()
    {
        _recentRepo.GetByUserIdAsync(UserId.New(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<RecentSearch>());

        var query = new GetRecentSearchesQuery()
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnCurrentUserSearches()
    {
        var userId = UserId.New();
        _recentRepo.GetByUserIdAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([RecentSearch.Create(userId, "mine", DateTimeOffset.UtcNow)]);

        var query = new GetRecentSearchesQuery()
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Query.Should().Be("mine");
    }
}
