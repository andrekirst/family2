using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Messaging.Application.Search;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class MessagingSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesByContent()
    {
        var messages = new List<Message>
        {
            CreateMessage("Hello everyone, welcome to the family!"),
            CreateMessage("Don't forget the groceries")
        };
        var repo = new FakeMessageRepository(messages);
        var provider = new MessagingSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "welcome");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Contain("welcome");
    }

    [Fact]
    public async Task SearchAsync_ReturnsRecentFirst()
    {
        var messages = new List<Message>
        {
            CreateMessage("First grocery list"),
            CreateMessage("Second grocery list")
        };
        var repo = new FakeMessageRepository(messages);
        var provider = new MessagingSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "grocery");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        var messages = Enumerable.Range(1, 20)
            .Select(i => CreateMessage($"Test message number {i}"))
            .ToList();
        var repo = new FakeMessageRepository(messages);
        var provider = new MessagingSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "test", Limit: 3);

        var results = await provider.SearchAsync(context);

        results.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public void ModuleName_ShouldBeMessaging()
    {
        var repo = new FakeMessageRepository();
        var provider = new MessagingSearchProvider(repo);

        provider.ModuleName.Should().Be("messaging");
    }

    private static Message CreateMessage(string content)
    {
        var msg = Message.Create(TestFamilyId, TestUserId, MessageContent.From(content));
        msg.ClearDomainEvents();
        return msg;
    }
}
