using System.Runtime.CompilerServices;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Subscriptions;

/// <summary>
/// GraphQL subscriptions for real-time family member and invitation updates.
/// Uses Redis PubSub for multi-instance coordination.
/// </summary>
[ExtendObjectType("Subscription")]
public sealed class InvitationSubscriptions
{
    /// <summary>
    /// Subscribe to real-time family member changes (ADDED, UPDATED, REMOVED).
    /// Requires family membership (any role).
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of family member changes.</returns>
    [Subscribe]
    [Topic("family-members-changed:{familyId}")]
    public async IAsyncEnumerable<FamilyMembersChangedPayload> FamilyMembersChanged(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] FamilyMembersChangedPayload message,
        [Service] ILogger<InvitationSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Authorization: User must be member of the family
        var currentUserId = userContext.UserId;
        var targetFamilyId = FamilyId.From(familyId);

        var user = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning(
                "User {UserId} not found for subscription authorization",
                currentUserId);
            yield break; // Unauthorized
        }

        // Check if user is member of the target family
        var isMember = user.FamilyId == targetFamilyId;

        if (!isMember)
        {
            logger.LogWarning(
                "User {UserId} attempted to subscribe to family {FamilyId} without membership",
                currentUserId,
                familyId);
            yield break; // Unauthorized
        }

        logger.LogInformation(
            "User {UserId} subscribed to family members changes for family {FamilyId}",
            currentUserId,
            familyId);

        // Yield the message (Hot Chocolate handles streaming)
        yield return message;
    }

    /// <summary>
    /// Subscribe to real-time pending invitation changes (ADDED, UPDATED, REMOVED).
    /// Requires OWNER or ADMIN role in the family.
    /// </summary>
    /// <param name="familyId">The family ID to subscribe to.</param>
    /// <param name="userContext">Current user context for authorization.</param>
    /// <param name="userRepository">User repository for family membership check.</param>
    /// <param name="message">Hot Chocolate event message (injected automatically).</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable stream of invitation changes.</returns>
    [Subscribe]
    [Topic("pending-invitations-changed:{familyId}")]
    public async IAsyncEnumerable<PendingInvitationsChangedPayload> PendingInvitationsChanged(
        Guid familyId,
        [Service] IUserContext userContext,
        [Service] IUserRepository userRepository,
        [EventMessage] PendingInvitationsChangedPayload message,
        [Service] ILogger<InvitationSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Authorization: User must be OWNER or ADMIN of the family
        var currentUserId = userContext.UserId;
        var targetFamilyId = FamilyId.From(familyId);

        var user = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning(
                "User {UserId} not found for subscription authorization",
                currentUserId);
            yield break; // Unauthorized
        }

        // Check if user is member of the target family
        if (user.FamilyId != targetFamilyId)
        {
            logger.LogWarning(
                "User {UserId} attempted to subscribe to invitations for family {FamilyId} without membership",
                currentUserId,
                familyId);
            yield break; // Unauthorized
        }

        // Check role: OWNER or ADMIN only
        if (user.Role != FamilyRole.Owner && user.Role != FamilyRole.Admin)
        {
            logger.LogWarning(
                "User {UserId} with role {Role} attempted to subscribe to invitations for family {FamilyId} (requires OWNER or ADMIN)",
                currentUserId,
                user.Role,
                familyId);
            yield break; // Unauthorized
        }

        logger.LogInformation(
            "User {UserId} subscribed to pending invitations changes for family {FamilyId}",
            currentUserId,
            familyId);

        // Yield the message (Hot Chocolate handles streaming)
        yield return message;
    }
}
