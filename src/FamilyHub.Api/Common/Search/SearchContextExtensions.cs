namespace FamilyHub.Api.Common.Search;

public static class SearchContextExtensions
{
    /// <summary>
    /// Checks if the search context's locale starts with the specified language code.
    /// </summary>
    public static bool IsLocale(this SearchContext context, string languageCode) =>
        context.Locale?.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase) == true;
}
