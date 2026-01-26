namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Client for Zitadel Management API operations.
/// </summary>
public interface IZitadelManagementApiClient
{
    /// <summary>
    /// Gets a user's profile from Zitadel.
    /// </summary>
    /// <param name="zitadelUserId">The Zitadel user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user profile, or null if not found.</returns>
    Task<ZitadelUserProfile?> GetUserProfileAsync(
        string zitadelUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's profile in Zitadel.
    /// </summary>
    /// <param name="zitadelUserId">The Zitadel user ID.</param>
    /// <param name="displayName">The new display name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> UpdateUserProfileAsync(
        string zitadelUserId,
        string displayName,
        CancellationToken cancellationToken = default);
}
