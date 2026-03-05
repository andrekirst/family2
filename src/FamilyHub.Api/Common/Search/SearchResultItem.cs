namespace FamilyHub.Api.Common.Search;

public sealed record SearchResultItem(
    string Title,
    string? Description,
    string Module,
    string Icon,
    string Route,
    Dictionary<string, string>? Metadata = null);
