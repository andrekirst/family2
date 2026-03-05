using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FamilyHub.Search.Tests;

public class UniversalSearchQueryHandlerTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldFanOutToAllProviders()
    {
        // Arrange
        var provider1 = new FakeSearchProvider("family");
        var provider2 = new FakeSearchProvider("calendar");
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler([provider1, provider2], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("test");

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        provider1.SearchCallCount.Should().Be(1);
        provider2.SearchCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithModuleFilter_ShouldOnlyQueryMatchingProviders()
    {
        // Arrange
        var familyProvider = new FakeSearchProvider("family");
        var calendarProvider = new FakeSearchProvider("calendar");
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler([familyProvider, calendarProvider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("test", modules: ["family"]);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        familyProvider.SearchCallCount.Should().Be(1);
        calendarProvider.SearchCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldLimitResults()
    {
        // Arrange
        var results = Enumerable.Range(1, 20)
            .Select(i => new SearchResultItem($"Item {i}", null, "family", "icon", "/route"))
            .ToList();
        var provider = new FakeSearchProvider("family", results);
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("test", limit: 5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ShouldFilterCommandsByPermissions()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("", permissions: ["family:invite"]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Commands.Should().Contain(c => c.Label == "No Perms Command");
        result.Commands.Should().Contain(c => c.Label == "Invite Command");
        result.Commands.Should().NotContain(c => c.Label == "Admin Command");
    }

    [Fact]
    public async Task Handle_ShouldFilterCommandsByKeywordMatch()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("invite", permissions: ["family:invite", "family:admin"]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Commands.Should().Contain(c => c.Label == "Invite Command");
        result.Commands.Should().NotContain(c => c.Label == "Admin Command");
    }

    [Fact]
    public async Task Handle_EmptyQuery_ShouldReturnAllCommands()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("", permissions: ["family:invite", "family:admin"]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Commands.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_NoProviders_ShouldReturnEmptyResults()
    {
        // Arrange
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler([], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("test");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapResultsToDto()
    {
        // Arrange
        var results = new List<SearchResultItem>
        {
            new("John Doe", "Owner", "family", "users", "/family/members/1")
        };
        var provider = new FakeSearchProvider("family", results);
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("john");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results[0].Title.Should().Be("John Doe");
        result.Results[0].Module.Should().Be("family");
        result.Results[0].Route.Should().Be("/family/members/1");
    }

    [Fact]
    public async Task Handle_ShouldCapResultsAt30()
    {
        // Arrange — 3 providers each returning 15 items = 45 total, should be capped at 30
        var results1 = Enumerable.Range(1, 15)
            .Select(i => new SearchResultItem($"Family {i}", null, "family", "icon", "/route"))
            .ToList();
        var results2 = Enumerable.Range(1, 15)
            .Select(i => new SearchResultItem($"Calendar {i}", null, "calendar", "icon", "/route"))
            .ToList();
        var results3 = Enumerable.Range(1, 15)
            .Select(i => new SearchResultItem($"Files {i}", null, "files", "icon", "/route"))
            .ToList();
        var provider1 = new FakeSearchProvider("family", results1);
        var provider2 = new FakeSearchProvider("calendar", results2);
        var provider3 = new FakeSearchProvider("files", results3);
        var registry = CreateEmptyRegistry();
        var handler = new UniversalSearchQueryHandler(
            [provider1, provider2, provider3], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("test", limit: 15);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().HaveCountLessOrEqualTo(30);
    }

    [Fact]
    public async Task Handle_WithGermanLocale_ShouldResolveGermanLabels()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithBilingualCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("einladen",
            permissions: ["family:invite"],
            locale: "de");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Commands.Should().Contain(c => c.Label == "Mitglied einladen");
    }

    [Fact]
    public async Task Handle_WithEnglishLocale_ShouldKeepEnglishLabels()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithBilingualCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("invite",
            permissions: ["family:invite"],
            locale: "en");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Commands.Should().Contain(c => c.Label == "Invite Member");
    }

    [Fact]
    public async Task Handle_WithGermanLocale_ShouldMatchGermanKeywords()
    {
        // Arrange
        var provider = new FakeSearchProvider("family");
        var registry = CreateRegistryWithBilingualCommands();
        var handler = new UniversalSearchQueryHandler([provider], registry, NullLogger<UniversalSearchQueryHandler>.Instance);
        var query = CreateQuery("mitglied",
            permissions: ["family:invite"],
            locale: "de");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — "mitglied" should match the German label "Mitglied einladen"
        result.Commands.Should().Contain(c => c.Label == "Mitglied einladen");
    }

    private static UniversalSearchQuery CreateQuery(
        string searchQuery,
        string[]? modules = null,
        int limit = 10,
        string[]? permissions = null,
        string? locale = null) =>
        new(TestUserId, TestFamilyId, searchQuery, modules, limit, permissions, locale);

    private static ICommandPaletteRegistry CreateEmptyRegistry() =>
        new CommandPaletteRegistry();

    private static ICommandPaletteRegistry CreateRegistryWithCommands()
    {
        var registry = new CommandPaletteRegistry();
        registry.RegisterProvider(new TestCommandProvider());
        return registry;
    }

    private static ICommandPaletteRegistry CreateRegistryWithBilingualCommands()
    {
        var registry = new CommandPaletteRegistry();
        registry.RegisterProvider(new BilingualCommandProvider());
        return registry;
    }

    private sealed class TestCommandProvider : ICommandPaletteProvider
    {
        public string ModuleName => "test";

        public IReadOnlyList<CommandDescriptor> GetCommands() =>
        [
            new CommandDescriptor("No Perms Command", "Open", ["open"], "/route", [], "icon", "test"),
            new CommandDescriptor("Invite Command", "Invite member", ["invite"], "/invite", ["family:invite"], "icon", "test"),
            new CommandDescriptor("Admin Command", "Admin only", ["admin"], "/admin", ["family:admin"], "icon", "test")
        ];
    }

    private sealed class BilingualCommandProvider : ICommandPaletteProvider
    {
        public string ModuleName => "family";

        public IReadOnlyList<CommandDescriptor> GetCommands() =>
        [
            new CommandDescriptor(
                "Invite Member", "Send an invitation to join your family",
                ["invite", "member", "add"],
                "/family?action=invite", ["family:invite"], "user-plus", "family",
                LabelDe: "Mitglied einladen",
                DescriptionDe: "Einladung zur Familie senden")
        ];
    }
}
