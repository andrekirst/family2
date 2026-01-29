using FamilyHub.Api.GraphQL.Types;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Api.Application.Services;

/// <summary>
/// Coordinates queries across Auth, Family, and UserProfile modules
/// for the consolidated "me" root query.
/// </summary>
/// <remarks>
/// <para>
/// This service acts as a cross-module coordinator in the API layer,
/// aggregating data from multiple modules while respecting module boundaries.
/// It does not violate DDD principles because it only orchestrates queries,
/// not domain logic.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
/// <item><description>Determining user's family status (has family, pending invite, left, none)</description></item>
/// <item><description>Fetching family data with appropriate visibility rules</description></item>
/// <item><description>Checking family membership for authorization</description></item>
/// <item><description>Getting profiles with role-based field filtering</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IMeQueryCoordinator
{
    /// <summary>
    /// Gets the current user's family or a reason why they have no family.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="FamilyOrReasonResult"/> that contains either the user's family
    /// or a reason explaining why they have no family (not created, invite pending, left).
    /// </returns>
    Task<FamilyOrReasonResult> GetFamilyOrReasonAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a user is a member of a specific family.
    /// </summary>
    /// <param name="familyId">The family ID to check membership in.</param>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is a member of the family, false otherwise.</returns>
    Task<bool> IsFamilyMemberAsync(Guid familyId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a family member's profile with role-based visibility filtering.
    /// </summary>
    /// <param name="userId">The user ID whose profile to fetch.</param>
    /// <param name="viewerRole">The role of the viewer (determines which fields are visible).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The user's profile with fields filtered based on the viewer's role:
    /// <list type="bullet">
    /// <item><description>OWNER/ADMIN: All fields visible</description></item>
    /// <item><description>MEMBER: FAMILY-level fields visible</description></item>
    /// <item><description>CHILD: PUBLIC-level fields only</description></item>
    /// </list>
    /// Returns null if the user doesn't exist or has no profile.
    /// </returns>
    Task<UserProfileDto?> GetProfileWithVisibilityAsync(
        Guid userId,
        FamilyRole viewerRole,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the count of pending invitations for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of pending invitations.</returns>
    Task<int> GetPendingInvitationCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile, or null if not found.</returns>
    Task<UserProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken);
}
