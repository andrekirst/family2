using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Calendar.Application.Search;
using FamilyHub.Api.Features.Calendar.Domain.Entities;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.Search.Tests;

public class CalendarSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesByTitle()
    {
        var events = new List<CalendarEvent>
        {
            CreateEvent("Team Meeting", null, null, DateTime.UtcNow.AddDays(1)),
            CreateEvent("Birthday Party", null, null, DateTime.UtcNow.AddDays(2))
        };
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "meeting");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Team Meeting");
    }

    [Fact]
    public async Task SearchAsync_MatchesByDescription()
    {
        var events = new List<CalendarEvent>
        {
            CreateEvent("Event A", "Discuss quarterly budget", null, DateTime.UtcNow.AddDays(1))
        };
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "budget");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Event A");
    }

    [Fact]
    public async Task SearchAsync_MatchesByLocation()
    {
        var events = new List<CalendarEvent>
        {
            CreateEvent("Meeting", null, "Conference Room B", DateTime.UtcNow.AddDays(1))
        };
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "conference");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ExcludesCancelledEvents()
    {
        var cancelledEvent = CreateEvent("Cancelled Meeting", null, null, DateTime.UtcNow.AddDays(1));
        cancelledEvent.Cancel();
        var events = new List<CalendarEvent> { cancelledEvent };
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "meeting");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        var events = Enumerable.Range(1, 15)
            .Select(i => CreateEvent($"Meeting {i}", null, null, DateTime.UtcNow.AddDays(i)))
            .ToList();
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "meeting", Limit: 5);

        var results = await provider.SearchAsync(context);

        results.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var events = new List<CalendarEvent>
        {
            CreateEvent("Team Meeting", null, null, DateTime.UtcNow.AddDays(1))
        };
        var repo = CreateRepo(events);
        var provider = new CalendarSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ModuleName_ShouldBeCalendar()
    {
        var repo = Substitute.For<ICalendarEventRepository>();
        var provider = new CalendarSearchProvider(repo);

        provider.ModuleName.Should().Be("calendar");
    }

    private static ICalendarEventRepository CreateRepo(List<CalendarEvent> events)
    {
        var repo = Substitute.For<ICalendarEventRepository>();
        repo.GetByFamilyAndDateRangeAsync(
                TestFamilyId,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(events.Where(e => e.FamilyId == TestFamilyId).OrderBy(e => e.StartTime).ToList());
        return repo;
    }

    private static CalendarEvent CreateEvent(
        string title, string? description, string? location, DateTime startTime)
    {
        var evt = CalendarEvent.Create(
            TestFamilyId, TestUserId,
            EventTitle.From(title), description, location,
            startTime, startTime.AddHours(1), false);
        evt.ClearDomainEvents();
        return evt;
    }
}
