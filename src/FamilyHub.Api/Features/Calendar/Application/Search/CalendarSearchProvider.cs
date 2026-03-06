using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Search;

public sealed class CalendarSearchProvider(ICalendarEventRepository calendarEventRepository) : ISearchProvider
{
    public string ModuleName => "calendar";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
        {
            return [];
        }

        var now = DateTime.UtcNow;
        var start = now.AddMonths(-3);
        var end = now.AddMonths(6);

        var events = await calendarEventRepository.GetByFamilyAndDateRangeAsync(
            context.FamilyId.Value, start, end, cancellationToken);

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;

        var filtered = events
            .Where(e => !e.IsCancelled)
            .Where(e => e.Title.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                        (e.Description?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true) ||
                        (e.Location?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true));

        // Prioritize upcoming events first, then past events by recency
        var sorted = filtered
            .OrderBy(e => e.StartTime < now ? 1 : 0)
            .ThenBy(e => e.StartTime < now ? DateTime.MaxValue.Ticks - e.StartTime.Ticks : e.StartTime.Ticks);

        return sorted
            .Take(context.Limit)
            .Select(e => new SearchResultItem(
                Title: e.Title.Value,
                Description: FormatEventDescription(e.StartTime, e.Location, isGerman),
                Module: "calendar",
                Icon: "calendar",
                Route: $"/calendar?event={e.Id.Value}"))
            .ToList()
            .AsReadOnly();
    }

    private static string FormatEventDescription(DateTime startTime, string? location, bool isGerman)
    {
        var dateStr = isGerman
            ? startTime.ToString("dd. MMM, HH:mm", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))
            : startTime.ToString("MMM dd, h:mm tt", System.Globalization.CultureInfo.InvariantCulture);

        return location is not null ? $"{dateStr} — {location}" : dateStr;
    }
}
