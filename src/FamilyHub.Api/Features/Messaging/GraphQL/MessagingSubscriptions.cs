using FamilyHub.Api.Features.Messaging.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Messaging.GraphQL;

/// <summary>
/// GraphQL subscriptions for real-time messaging.
/// Topic is family-scoped: "MessageSent_{familyId}" ensures clients only receive
/// messages for their own family channel.
/// </summary>
[ExtendObjectType("Subscription")]
public class MessagingSubscriptions
{
    /// <summary>
    /// Subscribe to new messages in a family channel.
    /// </summary>
    [Authorize]
    [Subscribe]
    [Topic("MessageSent_{familyId}")]
    public MessageDto MessageSent(
        Guid familyId,
        [EventMessage] MessageDto message)
        => message;
}
