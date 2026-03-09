using FamilyHub.Api.Features.FileManagement.Application.Queries.GetSavedSearches;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetSavedSearchesQueryHandlerTests
{
    private readonly ISavedSearchRepository _savedRepo = Substitute.For<ISavedSearchRepository>();
    private readonly GetSavedSearchesQueryHandler _handler;

    public GetSavedSearchesQueryHandlerTests()
    {
        _handler = new GetSavedSearchesQueryHandler(_savedRepo);
    }

    [Fact]
    public async Task Handle_ShouldReturnSavedSearches()
    {
        var userId = UserId.New();
        _savedRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns([
                SavedSearch.Create(userId, "Photos", "vacation", null),
                SavedSearch.Create(userId, "Docs", "invoice", null)
            ]);

        var query = new GetSavedSearchesQuery()
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoSavedSearches()
    {
        _savedRepo.GetByUserIdAsync(UserId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<SavedSearch>());

        var query = new GetSavedSearchesQuery()
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
