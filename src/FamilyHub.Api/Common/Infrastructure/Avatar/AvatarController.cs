using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// REST controller for serving avatar images.
/// Provides direct image binary with browser caching support (ETag, Cache-Control).
/// </summary>
[ApiController]
[Route("api/avatars")]
public class AvatarController(
    IAvatarRepository avatarRepository,
    IFileStorageService fileStorageService) : ControllerBase
{
    /// <summary>
    /// Get an avatar image variant by avatar ID and size.
    /// Returns the image binary with proper Content-Type and caching headers.
    /// </summary>
    [HttpGet("{avatarId:guid}/{size}")]
    [ResponseCache(Duration = 86400)] // 24 hours
    public async Task<IActionResult> GetAvatar(
        Guid avatarId,
        string size,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AvatarSize>(size, ignoreCase: true, out var avatarSize))
        {
            return BadRequest($"Invalid size '{size}'. Valid sizes: tiny, small, medium, large.");
        }

        AvatarId id;
        try
        {
            id = AvatarId.From(avatarId);
        }
        catch
        {
            return BadRequest("Invalid avatar ID.");
        }

        var avatar = await avatarRepository.GetByIdAsync(id, cancellationToken);
        if (avatar is null)
        {
            return NotFound();
        }

        var variant = avatar.GetVariant(avatarSize);
        if (variant is null)
        {
            return NotFound();
        }

        var data = await fileStorageService.GetAsync(variant.StorageKey, cancellationToken);
        if (data is null)
        {
            return NotFound();
        }

        // ETag based on storage key for cache validation
        var etag = $"\"{variant.StorageKey}\"";
        if (Request.Headers.IfNoneMatch.Contains(etag))
        {
            return StatusCode(304);
        }

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public, max-age=86400"; // 24 hours

        return File(data, variant.MimeType);
    }
}
