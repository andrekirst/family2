using FamilyHub.Modules.UserProfile.Domain.Constants;

namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents the synchronization status between a user profile and Zitadel identity provider.
/// Tracks whether the profile data is in sync, pending sync, or has a sync failure.
/// </summary>
[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct SyncStatus
{
    // IMPORTANT: ValidStatuses must be declared BEFORE the static readonly fields
    // that use From() to avoid static initialization order issues.
    private static readonly string[] ValidStatuses =
    [
        SyncStatusConstants.PendingValue,
        SyncStatusConstants.SyncedValue,
        SyncStatusConstants.FailedValue,
        SyncStatusConstants.PendingPushValue,
        SyncStatusConstants.PendingPullValue
    ];

    /// <summary>
    /// Pending - profile has not been synced with Zitadel yet.
    /// </summary>
    public static readonly SyncStatus Pending = From(SyncStatusConstants.PendingValue);

    /// <summary>
    /// Synced - profile is synchronized with Zitadel.
    /// </summary>
    public static readonly SyncStatus Synced = From(SyncStatusConstants.SyncedValue);

    /// <summary>
    /// Failed - last sync attempt failed, requires retry.
    /// </summary>
    public static readonly SyncStatus Failed = From(SyncStatusConstants.FailedValue);

    /// <summary>
    /// PendingPush - local changes are waiting to be pushed to Zitadel.
    /// </summary>
    public static readonly SyncStatus PendingPush = From(SyncStatusConstants.PendingPushValue);

    /// <summary>
    /// PendingPull - Zitadel changes are waiting to be pulled locally.
    /// </summary>
    public static readonly SyncStatus PendingPull = From(SyncStatusConstants.PendingPullValue);

    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Validation.Invalid("Sync status cannot be empty.");
        }

        if (!ValidStatuses.Contains(value.ToLowerInvariant()))
        {
            return Validation.Invalid($"Invalid sync status. Must be one of: {string.Join(", ", ValidStatuses)}");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Normalizes input by trimming and converting to lowercase.
    /// </summary>
    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();

    /// <summary>
    /// Indicates whether sync is needed (status is Pending, PendingPush, or PendingPull).
    /// </summary>
    public bool NeedsSync => this == Pending || this == PendingPush || this == PendingPull;

    /// <summary>
    /// Indicates whether the last sync attempt failed.
    /// </summary>
    public bool HasFailed => this == Failed;

    /// <summary>
    /// Indicates whether the profile is currently in sync with Zitadel.
    /// </summary>
    public bool IsSynced => this == Synced;
}
