using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;

/// <summary>
/// DTO representing the reconstructed profile state from event replay.
/// This is a mutable object used during event application.
/// </summary>
public sealed class ProfileStateDto
{
    /// <summary>
    /// The profile's unique identifier.
    /// </summary>
    public UserProfileId ProfileId { get; set; } = UserProfileId.From(Guid.Empty);

    /// <summary>
    /// The user who owns this profile.
    /// </summary>
    public UserId UserId { get; set; } = UserId.From(Guid.Empty);

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The user's birthday (optional).
    /// </summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>
    /// The user's preferred pronouns (optional).
    /// </summary>
    public string? Pronouns { get; set; }

    /// <summary>
    /// The user's language preference.
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// The user's timezone preference.
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// The user's date format preference.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Birthday field visibility level.
    /// </summary>
    public string BirthdayVisibility { get; set; } = "family";

    /// <summary>
    /// Pronouns field visibility level.
    /// </summary>
    public string PronounsVisibility { get; set; } = "family";

    /// <summary>
    /// Preferences field visibility level.
    /// </summary>
    public string PreferencesVisibility { get; set; } = "hidden";

    /// <summary>
    /// The current event version number.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// When the profile was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }
}
