using FamilyHub.Api.Common.Search;
using FamilyHub.EventChain.Domain.Repositories;

namespace FamilyHub.Api.Features.EventChain.Application.Search;

public sealed class EventChainSearchProvider(IChainDefinitionRepository chainDefinitionRepository) : ISearchProvider
{
    public string ModuleName => "event-chains";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.FamilyId is null || string.IsNullOrWhiteSpace(context.Query))
            return [];

        var definitions = await chainDefinitionRepository.GetByFamilyIdAsync(
            context.FamilyId.Value, ct: cancellationToken);

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;

        return definitions
            .Where(d => d.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                        (d.Description?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) == true))
            .Take(context.Limit)
            .Select(d => new SearchResultItem(
                Title: d.Name.Value,
                Description: FormatDescription(d.IsEnabled, d.TriggerEventType, isGerman),
                Module: "event-chains",
                Icon: "zap",
                Route: $"/event-chains?chain={d.Id.Value}"))
            .ToList()
            .AsReadOnly();
    }

    private static string FormatDescription(bool isEnabled, string triggerEventType, bool isGerman)
    {
        var status = isGerman
            ? (isEnabled ? "Aktiv" : "Inaktiv")
            : (isEnabled ? "Enabled" : "Disabled");
        var triggerLabel = isGerman ? "Auslöser" : "Trigger";
        return $"{status} — {triggerLabel}: {triggerEventType}";
    }
}
