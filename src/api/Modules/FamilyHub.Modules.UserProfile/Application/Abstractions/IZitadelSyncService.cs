using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

// Alias to disambiguate from GreenDonut.Result<T>
using Result = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.UserProfile.Application.Abstractions;

/// <summary>
/// Service for synchronizing user profile data with Zitadel identity provider.
/// Handles bidirectional sync of display name and other profile fields.
/// </summary>
public interface IZitadelSyncService
{
    /// <summary>
    /// Pushes the local display name to Zitadel.
    /// Called when the user updates their display name locally.
    /// </summary>
    /// <param name="userId">The internal user ID.</param>
    /// <param name="displayName">The display name to push to Zitadel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> PushDisplayNameAsync(
        UserId userId,
        DisplayName displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls the display name from Zitadel for the given external user ID.
    /// Called on user login to sync any changes made in Zitadel.
    /// </summary>
    /// <param name="externalUserId">The Zitadel user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The display name from Zitadel, or null if not found.</returns>
    Task<DisplayName?> PullDisplayNameAsync(
        string externalUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes the profile from Zitadel during user login.
    /// Applies last-write-wins conflict resolution.
    /// </summary>
    /// <param name="profile">The local user profile.</param>
    /// <param name="externalUserId">The Zitadel user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure, with updated profile if changed.</returns>
    Task<FamilyHub.SharedKernel.Domain.Result<SyncResult>> SyncFromZitadelAsync(
        Domain.Aggregates.UserProfile profile,
        string externalUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a sync operation.
/// </summary>
public sealed record SyncResult
{
    /// <summary>
    /// Indicates whether the profile was updated from Zitadel.
    /// </summary>
    public bool WasUpdated { get; init; }

    /// <summary>
    /// The display name after sync (may be same as before if not updated).
    /// </summary>
    public required DisplayName DisplayName { get; init; }

    /// <summary>
    /// Creates a result indicating no update was needed.
    /// </summary>
    public static SyncResult NoUpdate(DisplayName currentDisplayName) =>
        new() { WasUpdated = false, DisplayName = currentDisplayName };

    /// <summary>
    /// Creates a result indicating the profile was updated.
    /// </summary>
    public static SyncResult Updated(DisplayName newDisplayName) =>
        new() { WasUpdated = true, DisplayName = newDisplayName };
}
