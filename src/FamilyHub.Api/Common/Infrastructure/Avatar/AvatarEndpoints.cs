using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Minimal API endpoints for serving avatar images.
/// Provides direct image binary with browser caching support (ETag, Cache-Control).
/// </summary>
public static class AvatarEndpoints
{
    public static async Task<IResult> GetAvatar(
        Guid avatarId,
        string size,
        IAvatarRepository avatarRepository,
        IFileStorageService fileStorageService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AvatarSize>(size, ignoreCase: true, out var avatarSize))
        {
            return Results.BadRequest($"Invalid size '{size}'. Valid sizes: tiny, small, medium, large.");
        }

        AvatarId id;
        try
        {
            id = AvatarId.From(avatarId);
        }
        catch (Vogen.ValueObjectValidationException)
        {
            return Results.BadRequest("Invalid avatar ID.");
        }

        var avatar = await avatarRepository.GetByIdAsync(id, cancellationToken);
        if (avatar is null)
        {
            return Results.NotFound();
        }

        var variant = avatar.GetVariant(avatarSize);
        if (variant is null)
        {
            return Results.NotFound();
        }

        var data = await fileStorageService.GetAsync(variant.StorageKey, cancellationToken);
        if (data is null)
        {
            return Results.NotFound();
        }

        var etag = $"\"{variant.StorageKey}\"";
        if (httpContext.Request.Headers.IfNoneMatch.Contains(etag))
        {
            return Results.StatusCode(304);
        }

        httpContext.Response.Headers.ETag = etag;
        httpContext.Response.Headers.CacheControl = "public, max-age=86400";

        return Results.File(data, variant.MimeType);
    }
}
