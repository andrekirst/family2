namespace FamilyHub.Api.Common.Search;

public interface ISearchProvider
{
    string ModuleName { get; }
    Task<IReadOnlyList<SearchResultItem>> SearchAsync(SearchContext context, CancellationToken cancellationToken = default);
}
