using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Api.Application.Services;

/// <summary>
/// Determines field visibility based on the viewer's family role.
/// </summary>
/// <remarks>
/// <para>
/// This service implements role-based access control for profile fields:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Viewer Role</term>
/// <description>Visible Fields</description>
/// </listheader>
/// <item>
/// <term>OWNER / ADMIN</term>
/// <description>All fields (equivalent to profile owner view)</description>
/// </item>
/// <item>
/// <term>MEMBER</term>
/// <description>Fields marked as FAMILY or PUBLIC visibility</description>
/// </item>
/// <item>
/// <term>CHILD</term>
/// <description>Fields marked as PUBLIC visibility only</description>
/// </item>
/// </list>
/// </remarks>
public interface IRoleBasedVisibilityService
{
    /// <summary>
    /// Gets the maximum visibility level a viewer can access based on their role.
    /// </summary>
    /// <param name="viewerRole">The family role of the viewer.</param>
    /// <returns>
    /// The maximum visibility level:
    /// - OWNER/ADMIN → can see HIDDEN, FAMILY, PUBLIC fields
    /// - MEMBER → can see FAMILY, PUBLIC fields
    /// - CHILD → can see PUBLIC fields only
    /// </returns>
    string GetMaxVisibilityForRole(FamilyRole viewerRole);

    /// <summary>
    /// Filters profile fields based on the viewer's role.
    /// </summary>
    /// <param name="profile">The full profile with all fields.</param>
    /// <param name="viewerRole">The family role of the viewer.</param>
    /// <returns>
    /// A profile DTO with fields filtered based on visibility:
    /// - Fields that exceed the viewer's visibility level are set to null
    /// - Display name and timestamps are always visible
    /// </returns>
    UserProfileDto FilterFieldsByRole(UserProfileDto profile, FamilyRole viewerRole);

    /// <summary>
    /// Checks if a specific field should be visible to the viewer.
    /// </summary>
    /// <param name="fieldVisibility">The visibility setting of the field (HIDDEN, FAMILY, PUBLIC).</param>
    /// <param name="viewerRole">The family role of the viewer.</param>
    /// <returns>True if the field should be visible, false otherwise.</returns>
    bool IsFieldVisible(string fieldVisibility, FamilyRole viewerRole);
}
