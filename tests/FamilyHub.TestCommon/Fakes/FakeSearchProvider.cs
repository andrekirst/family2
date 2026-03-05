using FamilyHub.Api.Common.Search;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSearchProvider : ISearchProvider
{
    private readonly List<SearchResultItem> _results;
    public int SearchCallCount { get; private set; }
    public SearchContext? LastSearchContext { get; private set; }

    public FakeSearchProvider(string moduleName, List<SearchResultItem>? results = null)
    {
        ModuleName = moduleName;
        _results = results ?? [];
    }

    public string ModuleName { get; }

    public Task<IReadOnlyList<SearchResultItem>> SearchAsync(
        SearchContext context,
        CancellationToken cancellationToken = default)
    {
        SearchCallCount++;
        LastSearchContext = context;
        var limited = _results.Take(context.Limit).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<SearchResultItem>>(limited);
    }
}
