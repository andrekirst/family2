using System.Globalization;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyHub.Api.Common.Middleware;

/// <summary>
/// Middleware that resolves the request locale from:
/// 1. Cached user locale preference (5-minute sliding expiration)
/// 2. Authenticated user's DB preference (PreferredLocale)
/// 3. Accept-Language header
/// 4. Default "en"
/// Sets CultureInfo.CurrentCulture and CultureInfo.CurrentUICulture for the request.
/// </summary>
public class RequestLocaleResolutionMiddleware(RequestDelegate next)
{
    internal const string CacheKeyPrefix = "user-locale:";

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository, IMemoryCache memoryCache)
    {
        string locale = "en";

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var externalUserId = context.User.FindFirst(ClaimNames.Standard.Sub)?.Value;
            if (!string.IsNullOrEmpty(externalUserId) && Guid.TryParse(externalUserId, out var parsedId))
            {
                var cacheKey = $"{CacheKeyPrefix}{parsedId}";

                if (!memoryCache.TryGetValue(cacheKey, out string? cachedLocale))
                {
                    var user = await userRepository.GetByExternalIdAsync(
                        ExternalUserId.From(parsedId.ToString()), context.RequestAborted);

                    cachedLocale = user is not null && !string.IsNullOrEmpty(user.PreferredLocale)
                        ? user.PreferredLocale
                        : null;

                    if (cachedLocale is not null)
                    {
                        memoryCache.Set(cacheKey, cachedLocale, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        });
                    }
                }

                locale = cachedLocale ?? ResolveFromAcceptLanguage(context) ?? "en";
            }
        }
        else
        {
            locale = ResolveFromAcceptLanguage(context) ?? "en";
        }

        var culture = new CultureInfo(locale);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        await next(context);
    }

    private static string? ResolveFromAcceptLanguage(HttpContext context)
    {
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrEmpty(acceptLanguage)) return null;

        var languages = acceptLanguage.Split(',')
            .Select(l => l.Split(';')[0].Trim())
            .ToArray();

        return languages.Length > 0 ? languages[0] : null;
    }
}
