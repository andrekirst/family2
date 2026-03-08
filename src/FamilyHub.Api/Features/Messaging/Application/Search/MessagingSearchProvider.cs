using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;

namespace FamilyHub.Api.Features.Messaging.Application.Search;

public sealed class MessagingSearchProvider(IMessageRepository messageRepository) : ISearchProvider
{
    public string ModuleName => "messaging";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
        {
            return [];
        }

        var messages = await messageRepository.GetByFamilyAsync(
            context.FamilyId.Value, limit: 100, cancellationToken: cancellationToken);

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.IsLocale("de");

        return messages
            .Where(m => m.Content.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.SentAt)
            .Take(context.Limit)
            .Select(m => new SearchResultItem(
                Title: Truncate(m.Content.Value, 80),
                Description: FormatTimestamp(m.SentAt, isGerman),
                Module: "messaging",
                Icon: "message-circle",
                Route: $"/messages?message={m.Id.Value}"))
            .ToList()
            .AsReadOnly();
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength), "…");

    private static string FormatTimestamp(DateTime sentAt, bool isGerman) =>
        isGerman
            ? sentAt.ToString("dd. MMM, HH:mm", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))
            : sentAt.ToString("MMM dd, h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
}
