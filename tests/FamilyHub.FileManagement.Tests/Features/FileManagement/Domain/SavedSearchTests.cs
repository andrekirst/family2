using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class SavedSearchTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var userId = UserId.New();
        var name = "My Photos";
        var query = "vacation";
        var filtersJson = """{"mimeTypes":["image/jpeg"]}""";

        var search = SavedSearch.Create(userId, name, query, filtersJson, DateTimeOffset.UtcNow);

        search.UserId.Should().Be(userId);
        search.Name.Should().Be(name);
        search.Query.Should().Be(query);
        search.FiltersJson.Should().Be(filtersJson);
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var search = SavedSearch.Create(UserId.New(), "Old Name", "query", null, DateTimeOffset.UtcNow);

        search.Rename("New Name");

        search.Name.Should().Be("New Name");
    }
}
