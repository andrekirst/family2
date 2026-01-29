using System.Runtime.CompilerServices;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Api.GraphQL.Subscriptions;

/// <summary>
/// GraphQL subscriptions for real-time member lifecycle events.
/// Uses Redis PubSub for multi-instance coordination.
/// </summary>
/// <remarks>
/// <para>
/// These subscriptions enable real-time updates for family-related events:
/// <list type="bullet">
/// <item><description>Profile changes - when a family member updates their profile</description></item>
/// <item><description>Member joined - when a new member joins the family</description></item>
/// <item><description>Member left - when a member leaves the family</description></item>
/// <item><description>Role changed - when a member's role is updated</description></item>
/// </list>
/// </para>
/// <para>
/// All subscriptions require family membership for authorization.
/// </para>
/// </remarks>
[ExtendObjectType("Subscription")]
public sealed class MemberLifecycleSubscriptions
{
    /// <summary>
    /// Subscribe to profile changes for any family member.
    /// Requires family membership (any role).
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of profile change events.</returns>
    [Subscribe]
    [Topic("member-profile-changed:{familyId}")]
    [GraphQLDescription("Subscribe to profile changes for any family member.")]
    public async IAsyncEnumerable<MemberProfileChangedPayload> MemberProfileChanged(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] MemberProfileChangedPayload message,
        [Service] ILogger<MemberLifecycleSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!await ValidateFamilyMembershipAsync(
            familyId,
            userContext,
            userRepository,
            logger,
            "member-profile-changed",
            cancellationToken))
        {
            yield break;
        }

        yield return message;
    }

    /// <summary>
    /// Subscribe to member joined events.
    /// Requires family membership (any role).
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of member joined events.</returns>
    [Subscribe]
    [Topic("member-joined:{familyId}")]
    [GraphQLDescription("Subscribe to events when new members join the family.")]
    public async IAsyncEnumerable<MemberJoinedPayload> MemberJoined(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] MemberJoinedPayload message,
        [Service] ILogger<MemberLifecycleSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!await ValidateFamilyMembershipAsync(
            familyId,
            userContext,
            userRepository,
            logger,
            "member-joined",
            cancellationToken))
        {
            yield break;
        }

        yield return message;
    }

    /// <summary>
    /// Subscribe to member left events.
    /// Requires family membership (any role).
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of member left events.</returns>
    [Subscribe]
    [Topic("member-left:{familyId}")]
    [GraphQLDescription("Subscribe to events when members leave the family.")]
    public async IAsyncEnumerable<MemberLeftPayload> MemberLeft(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] MemberLeftPayload message,
        [Service] ILogger<MemberLifecycleSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!await ValidateFamilyMembershipAsync(
            familyId,
            userContext,
            userRepository,
            logger,
            "member-left",
            cancellationToken))
        {
            yield break;
        }

        yield return message;
    }

    /// <summary>
    /// Subscribe to role change events.
    /// Requires OWNER or ADMIN role in the family.
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of role change events.</returns>
    [Subscribe]
    [Topic("member-role-changed:{familyId}")]
    [GraphQLDescription("Subscribe to events when member roles change (requires OWNER or ADMIN).")]
    public async IAsyncEnumerable<MemberRoleChangedPayload> MemberRoleChanged(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] MemberRoleChangedPayload message,
        [Service] ILogger<MemberLifecycleSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Role change events require OWNER or ADMIN role
        if (!await ValidateFamilyMembershipAsync(
            familyId,
            userContext,
            userRepository,
            logger,
            "member-role-changed",
            cancellationToken,
            requireOwnerOrAdmin: true))
        {
            yield break;
        }

        yield return message;
    }

    /// <summary>
    /// Validates that the current user is a member of the specified family.
    /// </summary>
    private static async Task<bool> ValidateFamilyMembershipAsync(
        Guid familyId,
        IUserContext userContext,
        IUserRepository userRepository,
        ILogger logger,
        string subscriptionName,
        CancellationToken cancellationToken,
        bool requireOwnerOrAdmin = false)
    {
        var currentUserId = userContext.UserId;
        var targetFamilyId = FamilyId.From(familyId);

        var user = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning(
                "User {UserId} not found for {SubscriptionName} subscription authorization",
                currentUserId,
                subscriptionName);
            return false;
        }

        // Check if user is member of the target family
        if (user.FamilyId != targetFamilyId)
        {
            logger.LogWarning(
                "User {UserId} attempted to subscribe to {SubscriptionName} for family {FamilyId} without membership",
                currentUserId,
                subscriptionName,
                familyId);
            return false;
        }

        // Check role if required
        if (requireOwnerOrAdmin && user.Role != FamilyRole.Owner && user.Role != FamilyRole.Admin)
        {
            logger.LogWarning(
                "User {UserId} with role {Role} attempted to subscribe to {SubscriptionName} for family {FamilyId} (requires OWNER or ADMIN)",
                currentUserId,
                user.Role,
                subscriptionName,
                familyId);
            return false;
        }

        logger.LogInformation(
            "User {UserId} subscribed to {SubscriptionName} for family {FamilyId}",
            currentUserId,
            subscriptionName,
            familyId);

        return true;
    }
}
