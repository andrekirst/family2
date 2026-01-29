using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Api.Application.Services;

/// <summary>
/// Implements role-based visibility filtering for profile fields.
/// </summary>
/// <remarks>
/// Visibility matrix:
/// <code>
/// OWNER role   → sees all fields (HIDDEN, FAMILY, PUBLIC)
/// ADMIN role   → sees all fields (HIDDEN, FAMILY, PUBLIC)
/// MEMBER role  → sees FAMILY and PUBLIC fields
/// CHILD role   → sees PUBLIC fields only
/// </code>
/// </remarks>
public sealed class RoleBasedVisibilityService : IRoleBasedVisibilityService
{
    // Visibility levels in order of restrictiveness (most restrictive first)
    private const string VisibilityHidden = "HIDDEN";
    private const string VisibilityFamily = "FAMILY";
    private const string VisibilityPublic = "PUBLIC";

    /// <inheritdoc />
    public string GetMaxVisibilityForRole(FamilyRole viewerRole)
    {
        return viewerRole.Value.ToLowerInvariant() switch
        {
            "owner" => VisibilityHidden,  // Owner sees everything
            "admin" => VisibilityHidden,  // Admin sees everything
            "member" => VisibilityFamily, // Member sees family + public
            "child" => VisibilityPublic,  // Child sees public only
            _ => VisibilityPublic         // Default to most restrictive
        };
    }

    /// <inheritdoc />
    public UserProfileDto FilterFieldsByRole(UserProfileDto profile, FamilyRole viewerRole)
    {
        var maxVisibility = GetMaxVisibilityForRole(viewerRole);

        // If viewer can see everything, return the full profile
        if (maxVisibility == VisibilityHidden)
        {
            return profile;
        }

        // Filter fields based on their individual visibility settings
        return new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName, // Always visible
            Birthday = IsFieldVisible(profile.FieldVisibility.BirthdayVisibility, viewerRole)
                ? profile.Birthday
                : null,
            Age = IsFieldVisible(profile.FieldVisibility.BirthdayVisibility, viewerRole)
                ? profile.Age
                : null,
            Pronouns = IsFieldVisible(profile.FieldVisibility.PronounsVisibility, viewerRole)
                ? profile.Pronouns
                : null,
            Preferences = IsFieldVisible(profile.FieldVisibility.PreferencesVisibility, viewerRole)
                ? profile.Preferences
                : new ProfilePreferencesDto { Language = "", Timezone = "", DateFormat = "" },
            FieldVisibility = profile.FieldVisibility, // Visibility settings themselves are always visible
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    /// <inheritdoc />
    public bool IsFieldVisible(string fieldVisibility, FamilyRole viewerRole)
    {
        var maxVisibility = GetMaxVisibilityForRole(viewerRole);
        var fieldLevel = GetVisibilityLevel(fieldVisibility);
        var viewerLevel = GetVisibilityLevel(maxVisibility);

        // Field is visible if its level is >= viewer's max level
        // (higher level = more public = less restrictive)
        return fieldLevel >= viewerLevel;
    }

    /// <summary>
    /// Converts visibility string to a numeric level for comparison.
    /// Higher number = more public (less restrictive).
    /// </summary>
    private static int GetVisibilityLevel(string visibility) =>
        visibility.ToUpperInvariant() switch
        {
            VisibilityHidden => 0, // Most restrictive
            VisibilityFamily => 1,
            VisibilityPublic => 2, // Least restrictive
            _ => 0                 // Unknown = treat as hidden
        };
}
