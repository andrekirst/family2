using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class RecentSearchTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var userId = UserId.New();
        var query = "vacation photos";

        var search = RecentSearch.Create(userId, query);

        search.UserId.Should().Be(userId);
        search.Query.Should().Be(query);
        search.SearchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
