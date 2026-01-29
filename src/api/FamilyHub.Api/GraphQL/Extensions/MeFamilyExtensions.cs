using FamilyHub.Api.Application.Services;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Api.GraphQL.Extensions;

/// <summary>
/// Extends Family type with role-based profile access when accessed via me.family context.
/// </summary>
/// <remarks>
/// <para>
/// This extension adds the `profile(userId: ID!)` field to the Family type,
/// enabling family members to view each other's profiles with role-based
/// visibility filtering.
/// </para>
/// <para>
/// Visibility rules:
/// <list type="bullet">
/// <item><description>OWNER/ADMIN: See all profile fields</description></item>
/// <item><description>MEMBER: See FAMILY and PUBLIC fields</description></item>
/// <item><description>CHILD: See PUBLIC fields only</description></item>
/// </list>
/// </para>
/// </remarks>
[ExtendObjectType("Family")]
public sealed class MeFamilyExtensions
{
    /// <summary>
    /// Gets a family member's profile with role-based visibility.
    /// </summary>
    /// <param name="family">The family from the parent resolver.</param>
    /// <param name="userId">The global ID of the user whose profile to fetch.</param>
    /// <param name="coordinator">The cross-module query coordinator.</param>
    /// <param name="userContext">The current user's context (provides viewer role).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The user's profile with fields filtered based on the viewer's role.
    /// Returns null if the profile doesn't exist.
    /// Throws ACCESS_DENIED if the userId is not a member of this family.
    /// </returns>
    [Authorize]
    [GraphQLDescription("Get a family member's profile with role-based visibility.")]
    public async Task<UserProfileDto?> Profile(
        [Parent] FamilyAggregate family,
        [ID("User")] Guid userId,
        [Service] IMeQueryCoordinator coordinator,
        [Service] IUserContext userContext,
        CancellationToken cancellationToken)
    {
        // 1. Verify the requested user is a member of this family
        var isMember = await coordinator.IsFamilyMemberAsync(
            family.Id.Value,
            userId,
            cancellationToken);

        if (!isMember)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("User is not a member of this family.")
                    .SetCode("ACCESS_DENIED")
                    .SetExtension("userId", GlobalIdSerializer.Serialize("User", userId))
                    .SetExtension("familyId", GlobalIdSerializer.Serialize("Family", family.Id.Value))
                    .Build());
        }

        // 2. Get profile with role-based visibility filtering
        return await coordinator.GetProfileWithVisibilityAsync(
            userId,
            userContext.Role,
            cancellationToken);
    }
}
