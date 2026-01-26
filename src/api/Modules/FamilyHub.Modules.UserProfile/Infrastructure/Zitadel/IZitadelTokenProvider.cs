namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Provides access tokens for Zitadel Management API authentication.
/// </summary>
public interface IZitadelTokenProvider
{
    /// <summary>
    /// Gets an access token for the Zitadel Management API.
    /// Uses caching to avoid unnecessary token requests.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A valid access token.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cached token, forcing a new token to be obtained on next request.
    /// </summary>
    void InvalidateToken();
}
