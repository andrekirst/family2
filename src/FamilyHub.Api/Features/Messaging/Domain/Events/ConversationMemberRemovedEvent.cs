using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Events;

/// <summary>
/// Raised when a member is removed from a conversation.
/// </summary>
public sealed record ConversationMemberRemovedEvent(
    ConversationId ConversationId,
    UserId UserId,
    FamilyId FamilyId
) : DomainEvent;
