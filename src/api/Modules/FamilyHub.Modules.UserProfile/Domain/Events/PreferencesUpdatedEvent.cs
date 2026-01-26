using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a user's preferences (language, timezone) are updated.
/// Triggers event chain: Future Notifications module integration.
/// </summary>
public sealed class PreferencesUpdatedEvent(
    int eventVersion,
    UserProfileId profileId,
    UserId userId,
    string? oldLanguage,
    string? newLanguage,
    string? oldTimezone,
    string? newTimezone)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique identifier for the profile.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// User ID that owns this profile.
    /// </summary>
    public UserId UserId { get; } = userId;

    /// <summary>
    /// The previous language preference (ISO 639-1 code, e.g., "en", "de").
    /// </summary>
    public string? OldLanguage { get; } = oldLanguage;

    /// <summary>
    /// The new language preference (ISO 639-1 code, e.g., "en", "de").
    /// </summary>
    public string? NewLanguage { get; } = newLanguage;

    /// <summary>
    /// The previous timezone preference (IANA timezone ID, e.g., "America/New_York").
    /// </summary>
    public string? OldTimezone { get; } = oldTimezone;

    /// <summary>
    /// The new timezone preference (IANA timezone ID, e.g., "America/New_York").
    /// </summary>
    public string? NewTimezone { get; } = newTimezone;
}
