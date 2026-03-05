using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Domain.Events;

/// <summary>
/// Raised when a new conversation is created.
/// Can trigger folder creation for conversation attachments.
/// </summary>
public sealed record ConversationCreatedEvent(
    ConversationId ConversationId,
    FamilyId FamilyId,
    ConversationName Name,
    ConversationType Type,
    UserId CreatedBy
) : DomainEvent;
