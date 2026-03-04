using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.EventChain.Application.Search;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class EventChainSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesByName()
    {
        var definitions = new List<ChainDefinition>
        {
            CreateDefinition("Welcome Email Chain", "Sends welcome email to new members"),
            CreateDefinition("Birthday Reminder", "Reminds family about upcoming birthdays")
        };
        var repo = new FakeChainDefinitionRepository(definitions);
        var provider = new EventChainSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "welcome");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Welcome Email Chain");
    }

    [Fact]
    public async Task SearchAsync_MatchesByDescription()
    {
        var definitions = new List<ChainDefinition>
        {
            CreateDefinition("Chain A", "Sends birthday notifications")
        };
        var repo = new FakeChainDefinitionRepository(definitions);
        var provider = new EventChainSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "birthday");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_IncludesEnabledAndDisabled()
    {
        var enabled = CreateDefinition("Enabled Chain", "Active workflow");
        var disabled = CreateDefinition("Disabled Chain", "Inactive workflow");
        disabled.Disable();
        var definitions = new List<ChainDefinition> { enabled, disabled };
        var repo = new FakeChainDefinitionRepository(definitions);
        var provider = new EventChainSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "chain");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(2);
    }

    [Fact]
    public void ModuleName_ShouldBeEventChains()
    {
        var repo = new FakeChainDefinitionRepository();
        var provider = new EventChainSearchProvider(repo);

        provider.ModuleName.Should().Be("event-chains");
    }

    private static ChainDefinition CreateDefinition(string name, string? description)
    {
        var definition = ChainDefinition.Create(
            ChainName.From(name),
            description,
            TestFamilyId,
            TestUserId,
            "family.UserRegistered",
            "family",
            null,
            null);
        return definition;
    }
}
