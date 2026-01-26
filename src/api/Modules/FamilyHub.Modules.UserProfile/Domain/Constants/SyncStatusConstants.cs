namespace FamilyHub.Modules.UserProfile.Domain.Constants;

/// <summary>
/// Constants for sync status values.
/// Used by SyncStatus value object to ensure consistent values for Zitadel synchronization tracking.
/// </summary>
public static class SyncStatusConstants
{
    /// <summary>
    /// Pending - profile has not been synced with Zitadel yet.
    /// </summary>
    public const string PendingValue = "pending";

    /// <summary>
    /// Synced - profile is synchronized with Zitadel.
    /// </summary>
    public const string SyncedValue = "synced";

    /// <summary>
    /// Failed - last sync attempt failed, requires retry.
    /// </summary>
    public const string FailedValue = "failed";

    /// <summary>
    /// PendingPush - local changes are waiting to be pushed to Zitadel.
    /// </summary>
    public const string PendingPushValue = "pending_push";

    /// <summary>
    /// PendingPull - Zitadel changes are waiting to be pulled locally.
    /// </summary>
    public const string PendingPullValue = "pending_pull";
}
