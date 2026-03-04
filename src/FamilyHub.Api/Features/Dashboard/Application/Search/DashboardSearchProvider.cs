using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;

namespace FamilyHub.Api.Features.Dashboard.Application.Search;

public sealed class DashboardSearchProvider(IDashboardLayoutRepository dashboardLayoutRepository) : ISearchProvider
{
    public string ModuleName => "dashboard";

    public async Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.Query))
            return [];

        var queryLower = context.Query.ToLowerInvariant();
        var isGerman = context.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;
        var results = new List<SearchResultItem>();

        // Search personal dashboard
        var personal = await dashboardLayoutRepository.GetPersonalDashboardAsync(
            context.UserId, cancellationToken);
        if (personal is not null &&
            personal.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new SearchResultItem(
                Title: personal.Name.Value,
                Description: isGerman ? "Persönliches Dashboard" : "Personal dashboard",
                Module: "dashboard",
                Icon: "layout-dashboard",
                Route: "/dashboard"));
        }

        // Search shared family dashboard
        if (context.FamilyId is not null)
        {
            var shared = await dashboardLayoutRepository.GetSharedDashboardAsync(
                context.FamilyId.Value, cancellationToken);
            if (shared is not null &&
                shared.Name.Value.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new SearchResultItem(
                    Title: shared.Name.Value,
                    Description: isGerman ? "Geteiltes Familien-Dashboard" : "Shared family dashboard",
                    Module: "dashboard",
                    Icon: "layout-dashboard",
                    Route: "/dashboard"));
            }
        }

        return results
            .Take(context.Limit)
            .ToList()
            .AsReadOnly();
    }
}
