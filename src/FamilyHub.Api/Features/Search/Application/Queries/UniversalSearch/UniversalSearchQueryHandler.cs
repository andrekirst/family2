using FamilyHub.Api.Common.Search;
using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Search.Models;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Api.Features.Search.Application.Queries.UniversalSearch;

public sealed class UniversalSearchQueryHandler(
    IEnumerable<ISearchProvider> searchProviders,
    ICommandPaletteRegistry commandRegistry,
    ILogger<UniversalSearchQueryHandler> logger)
    : IQueryHandler<UniversalSearchQuery, UniversalSearchResult>
{
    private const int OverallResultCap = 30;

    public async ValueTask<UniversalSearchResult> Handle(
        UniversalSearchQuery query,
        CancellationToken cancellationToken)
    {
        // Filter providers by requested modules (if specified)
        var providers = searchProviders.AsEnumerable();
        if (query.Modules is { Length: > 0 })
        {
            var moduleSet = new HashSet<string>(query.Modules, StringComparer.OrdinalIgnoreCase);
            providers = providers.Where(p => moduleSet.Contains(p.ModuleName));
        }

        // Run search providers sequentially — they share the same scoped DbContext
        var searchContext = new SearchContext(
            query.UserId, query.FamilyId, query.Query, query.Limit, query.Locale);
        var searchResults = new List<IReadOnlyList<SearchResultItem>>();
        foreach (var provider in providers)
        {
            try
            {
                searchResults.Add(await provider.SearchAsync(searchContext, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search provider {Module} failed for query '{Query}'",
                    provider.ModuleName, searchContext.Query);
            }
        }

        // Flatten and apply overall cap (30 total)
        var results = searchResults
            .SelectMany(r => r)
            .Take(OverallResultCap)
            .Select(r => new SearchResultItemDto(r.Title, r.Description, r.Module, r.Icon, r.Route))
            .ToList()
            .AsReadOnly();

        // Resolve locale for command labels
        var isGerman = query.Locale?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true;

        // Filter commands by keyword match and permissions
        var queryLower = query.Query.ToLowerInvariant();
        var userPermissions = query.UserPermissions is { Length: > 0 }
            ? new HashSet<string>(query.UserPermissions)
            : new HashSet<string>();

        var commands = commandRegistry.GetAllCommands()
            .Where(c => MatchesKeyword(c, queryLower))
            .Where(c => c.RequiredPermissions.Length == 0 ||
                        c.RequiredPermissions.All(p => userPermissions.Contains(p)))
            .Select(c => isGerman ? ResolveGermanLabels(c) : c)
            .ToList()
            .AsReadOnly();

        return new UniversalSearchResult(results, commands);
    }

    private static bool MatchesKeyword(CommandDescriptor command, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        return command.Label.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               command.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               command.Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
               (command.LabelDe?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
               (command.DescriptionDe?.Contains(query, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static CommandDescriptor ResolveGermanLabels(CommandDescriptor command) =>
        command with
        {
            Label = command.LabelDe ?? command.Label,
            Description = command.DescriptionDe ?? command.Description
        };
}
