using FamilyHub.Api.Features.FileManagement.Application.Queries.GetSavedSearches;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetSavedSearchesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSavedSearches()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new GetSavedSearchesQueryHandler(savedRepo);

        var userId = UserId.New();
        savedRepo.Searches.Add(SavedSearch.Create(userId, "Photos", "vacation", null));
        savedRepo.Searches.Add(SavedSearch.Create(userId, "Docs", "invoice", null));

        var query = new GetSavedSearchesQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoSavedSearches()
    {
        var savedRepo = new FakeSavedSearchRepository();
        var handler = new GetSavedSearchesQueryHandler(savedRepo);

        var query = new GetSavedSearchesQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
