using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Events;

/// <summary>
/// Raised when a member is added to a conversation.
/// </summary>
public sealed record ConversationMemberAddedEvent(
    ConversationId ConversationId,
    UserId UserId,
    FamilyId FamilyId
) : DomainEvent;
