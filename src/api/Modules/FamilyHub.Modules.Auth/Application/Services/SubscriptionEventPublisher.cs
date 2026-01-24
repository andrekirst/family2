using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Services;

/// <summary>
/// Helper service for publishing GraphQL subscription messages from command handlers.
/// </summary>
/// <remarks>
/// <para>
/// This service provides convenience methods for publishing subscription messages to Redis PubSub.
/// It handles topic name generation and payload construction, making it easier for command handlers
/// to trigger real-time updates.
/// </para>
/// <para>
/// <strong>Architecture Note:</strong>
/// </para>
/// <para>
/// Ideally, subscription messages should be triggered by domain events (raised in aggregates).
/// However, since the current codebase doesn't raise domain events in aggregates yet, this service
/// provides a pragmatic interim solution where command handlers can directly publish subscription
/// messages after successful operations.
/// </para>
/// <para>
/// <strong>Future Refactoring:</strong>
/// </para>
/// <para>
/// When domain events are implemented in aggregates (e.g., User.AcceptInvitation() raises
/// InvitationAcceptedEvent), this service can be replaced with proper domain event handlers
/// (INotificationHandler&lt;TEvent&gt;) that listen to domain events and publish subscription messages.
/// </para>
/// <para>
/// <strong>Usage in Command Handlers:</strong>
/// </para>
/// <code>
/// // In AcceptInvitationCommandHandler.Handle():
/// await unitOfWork.SaveChangesAsync(cancellationToken);
///
/// // Publish subscription message for real-time UI updates
/// await subscriptionPublisher.PublishFamilyMemberAddedAsync(
///     invitation.FamilyId,
///     currentUser,
///     cancellationToken
/// );
/// </code>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="SubscriptionEventPublisher"/> class.
/// </remarks>
/// <param name="redisPublisher">The Redis subscription publisher.</param>
/// <param name="logger">The logger.</param>
public sealed class SubscriptionEventPublisher(
    IRedisSubscriptionPublisher redisPublisher,
    ILogger<SubscriptionEventPublisher> logger)
{
    private readonly IRedisSubscriptionPublisher _redisPublisher = redisPublisher;
    private readonly ILogger<SubscriptionEventPublisher> _logger = logger;

    /// <summary>
    /// Publishes a "family member added" subscription message.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="member">The family member details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task PublishFamilyMemberAddedAsync(
        FamilyId familyId,
        FamilyMemberType member,
        CancellationToken cancellationToken = default)
    {
        var payload = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Member = member
        };

        var topicName = $"family-members-changed:{familyId.Value}";
        await _redisPublisher.PublishAsync(topicName, payload, cancellationToken);

        _logger.LogDebug(
            "Published family member ADDED subscription for family {FamilyId}",
            familyId.Value);
    }

    /// <summary>
    /// Publishes a "family member removed" subscription message.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="memberId">The removed member's user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task PublishFamilyMemberRemovedAsync(
        FamilyId familyId,
        UserId memberId,
        CancellationToken cancellationToken = default)
    {
        var payload = new FamilyMembersChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.REMOVED,
            Member = null // Member data not available for removed members
        };

        var topicName = $"family-members-changed:{familyId.Value}";
        await _redisPublisher.PublishAsync(topicName, payload, cancellationToken);

        _logger.LogDebug(
            "Published family member REMOVED subscription for family {FamilyId}, member {MemberId}",
            familyId.Value,
            memberId.Value);
    }

    /// <summary>
    /// Publishes a "pending invitation added" subscription message.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="invitation">The invitation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task PublishInvitationAddedAsync(
        FamilyId familyId,
        PendingInvitationType invitation,
        CancellationToken cancellationToken = default)
    {
        var payload = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.ADDED,
            Invitation = invitation
        };

        var topicName = $"pending-invitations-changed:{familyId.Value}";
        await _redisPublisher.PublishAsync(topicName, payload, cancellationToken);

        _logger.LogDebug(
            "Published pending invitation ADDED subscription for family {FamilyId}",
            familyId.Value);
    }

    /// <summary>
    /// Publishes a "pending invitation removed" subscription message.
    /// Triggered when an invitation is accepted or canceled.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="invitationToken">The invitation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task PublishInvitationRemovedAsync(
        FamilyId familyId,
        string invitationToken,
        CancellationToken cancellationToken = default)
    {
        var payload = new PendingInvitationsChangedPayload
        {
            FamilyId = familyId.Value,
            ChangeType = ChangeType.REMOVED,
            Invitation = null // Invitation no longer pending
        };

        var topicName = $"pending-invitations-changed:{familyId.Value}";
        await _redisPublisher.PublishAsync(topicName, payload, cancellationToken);

        _logger.LogDebug(
            "Published pending invitation REMOVED subscription for family {FamilyId}, token {Token}",
            familyId.Value,
            invitationToken);
    }
}
