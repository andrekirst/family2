using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Result of creating a conversation.
/// </summary>
public sealed record CreateConversationResult(
    ConversationId ConversationId,
    Conversation Conversation
);
