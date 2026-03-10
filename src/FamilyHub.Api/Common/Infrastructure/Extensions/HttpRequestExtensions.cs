namespace FamilyHub.Api.Common.Infrastructure.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Extracts the preferred locale from the Accept-Language header.
    /// Returns the first language tag (ignoring quality values), or null if not present.
    /// </summary>
    public static string? GetPreferredLocale(this HttpRequest request)
    {
        var acceptLanguage = request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrEmpty(acceptLanguage)) return null;

        var languages = acceptLanguage.Split(',')
            .Select(l => l.Split(';')[0].Trim())
            .ToArray();

        return languages.Length > 0 ? languages[0] : null;
    }
}
