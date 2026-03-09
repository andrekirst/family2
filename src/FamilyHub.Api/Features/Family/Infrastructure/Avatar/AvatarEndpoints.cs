using FamilyHub.Api.Common.Infrastructure.ErrorMapping;
using FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.Family.Infrastructure.Avatar;

/// <summary>
/// Minimal API endpoints for serving avatar images.
/// Delegates business logic to GetAvatarQuery via IQueryBus.
/// Keeps HTTP-level concerns: ETag negotiation, Cache-Control headers, 304 responses.
/// </summary>
public static class AvatarEndpoints
{
    public static async Task<IResult> GetAvatar(
        Guid avatarId,
        string size,
        IQueryBus queryBus,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetAvatarQuery(avatarId, size);
        var result = await queryBus.QueryAsync(query, cancellationToken);

        return result.Match(
            success =>
            {
                // ETag / 304 Not Modified (HTTP transport concern)
                if (httpContext.Request.Headers.IfNoneMatch.Contains(success.ETag))
                {
                    success.Data.Dispose();
                    return Results.StatusCode(304);
                }

                httpContext.Response.Headers.ETag = success.ETag;
                httpContext.Response.Headers.CacheControl = "public, max-age=86400";

                return Results.File(success.Data, success.MimeType);
            },
            error => (IResult)DomainErrorToProblemDetailsMapper.ToProblemDetails(error));
    }
}
